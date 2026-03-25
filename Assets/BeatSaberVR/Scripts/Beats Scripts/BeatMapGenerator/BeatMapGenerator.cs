using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BeatSaberVR
{
    public class BeatMapGenerator : MonoBehaviour
    {
        [Header("Target")]
        public BeatMapSO targetBeatMap;
        public AudioClip songClip;
        public float bpm = 128f;
        public float noteJumpSpeed = 10f;

        [Header("Beat Detection")]
        [Tooltip("Window size for energy analysis — smaller = more sensitive")]
        public int windowSize = 1024;
        [Tooltip("How much louder than average a window must be to count as a beat")]
        [Range(1.1f, 3f)]
        public float beatThreshold = 1.4f;
        [Tooltip("Minimum seconds between any two detected beats")]
        public float minBeatGap = 0.35f;

        [Header("Note Spacing")]
        [Tooltip("Minimum seconds between notes of the SAME hand")]
        public float minSameHandGap = 0.6f;
        [Tooltip("Minimum seconds between ANY two notes (prevents visual overlap)")]
        public float minAnyNoteGap = 0.35f;
        [Tooltip("Chance 0-1 that a detected beat actually becomes a note")]
        [Range(0.3f, 1f)]
        public float noteDensity = 0.75f;

        [Header("Walls")]
        [Range(0f, 1f)]
        [Tooltip("Chance per eligible beat that a wall spawns")]
        public float wallChance = 0.12f;
        [Tooltip("Minimum seconds between walls")]
        public float minWallGap = 6f;
        [Tooltip("Only spawn walls on strong beats (high energy)")]
        public float wallEnergyMinRatio = 1.6f;

        [Header("Cut Direction Sync")]
        [Tooltip("Sync cut directions to beat position in measure")]
        public bool syncDirectionsToBeats = true;
        [Range(0f, 1f)]
        [Tooltip("How strongly beat energy influences direction (0=random, 1=fully synced)")]
        public float directionSyncStrength = 0.8f;

        [Header("Padding")]
        public float startPadding = 8f;
        public float endPadding = 3f;

        // Lane pools
        private static readonly int[,] leftHandLanes = {
            { 0, 0 }, { 0, 1 }, { 0, 2 },
            { 1, 0 }, { 1, 1 }, { 1, 2 }
        };
        private static readonly int[,] rightHandLanes = {
            { 2, 0 }, { 2, 1 }, { 2, 2 },
            { 3, 0 }, { 3, 1 }, { 3, 2 }
        };

        [ContextMenu("Generate Into ScriptableObject")]
        public void Generate()
        {
            if (targetBeatMap == null || songClip == null)
            { Debug.LogError("Assign BeatMapSO and AudioClip."); return; }

            // ── Step 1: Extract and convert to mono ────────────────
            float[] samples = new float[songClip.samples * songClip.channels];
            songClip.GetData(samples, 0);

            int channels = songClip.channels;
            int sampleRate = songClip.frequency;
            float songLength = songClip.length;
            int monoLength = songClip.samples;

            float[] mono = new float[monoLength];
            for (int i = 0; i < monoLength; i++)
            {
                float s = 0f;
                for (int c = 0; c < channels; c++) s += samples[i * channels + c];
                mono[i] = s / channels;
            }

            // ── Step 2: RMS energy per window ──────────────────────
            int numWindows = monoLength / windowSize;
            float[] energy = new float[numWindows];

            for (int w = 0; w < numWindows; w++)
            {
                float s = 0f;
                int off = w * windowSize;
                for (int i = off; i < off + windowSize && i < monoLength; i++)
                    s += mono[i] * mono[i];
                energy[w] = Mathf.Sqrt(s / windowSize);
            }

            // ── Step 3: Rolling average energy ─────────────────────
            int hist = Mathf.Max(1, Mathf.RoundToInt((float)sampleRate / windowSize));
            float[] avgEnergy = new float[numWindows];

            for (int w = 0; w < numWindows; w++)
            {
                int from = Mathf.Max(0, w - hist);
                int to = Mathf.Min(numWindows - 1, w + hist);
                float s = 0f;
                for (int i = from; i <= to; i++) s += energy[i];
                avgEnergy[w] = s / (to - from + 1);
            }

            // ── Step 4: Detect beats with energy context ────────────
            var beatList = new List<(float time, float ratio, float delta)>();
            float lastBeat = -99f;

            for (int w = 1; w < numWindows - 1; w++)
            {
                float time = (float)(w * windowSize) / sampleRate;

                if (time < startPadding || time > songLength - endPadding) continue;

                bool isPeak = energy[w] > energy[w - 1] && energy[w] > energy[w + 1];
                float ratio = avgEnergy[w] > 0 ? energy[w] / avgEnergy[w] : 0f;
                bool aboveThres = ratio >= beatThreshold;
                bool gapOk = time - lastBeat >= minBeatGap;

                if (isPeak && aboveThres && gapOk)
                {
                    // Energy delta: positive = energy is rising into this beat
                    float delta = energy[w] - energy[Mathf.Max(0, w - 2)];
                    beatList.Add((time, ratio, delta));
                    lastBeat = time;
                }
            }

            Debug.Log($"Beat detection: {beatList.Count} beats in {songLength:F1}s");

            // ── Step 5: Convert beats → notes ──────────────────────
            var rng = new System.Random(42);
            var notes = new List<NoteData>();
            var obstacles = new List<ObstacleData>();

            float lastLeftTime = -99f, lastRightTime = -99f, lastAnyTime = -99f;
            float lastObsTime = -99f;
            int lastLeftLane = -1, lastRightLane = -1;
            float spb = 60f / bpm; // seconds per beat

            for (int bi = 0; bi < beatList.Count; bi++)
            {
                var (beatTime, energyRatio, energyDelta) = beatList[bi];

                if (rng.NextDouble() > noteDensity) continue;

                bool leftReady = (beatTime - lastLeftTime >= minSameHandGap) &&
                                  (beatTime - lastAnyTime >= minAnyNoteGap);
                bool rightReady = (beatTime - lastRightTime >= minSameHandGap) &&
                                  (beatTime - lastAnyTime >= minAnyNoteGap);

                if (!leftReady && !rightReady) continue;

                // Beat position within a 4/4 measure (0=downbeat, 1=beat2, 2=beat3, 3=beat4)
                int beatInMeasure = Mathf.FloorToInt((beatTime / spb) % 4f);

                int choice = rng.Next(0, 3); // 0=left only, 1=right only, 2=both

                // ── Left hand ──────────────────────────────────────
                if (leftReady && (choice == 0 || choice == 2))
                {
                    int laneIdx;
                    do { laneIdx = rng.Next(0, leftHandLanes.GetLength(0)); }
                    while (laneIdx == lastLeftLane && leftHandLanes.GetLength(0) > 1);

                    int col = leftHandLanes[laneIdx, 0];
                    int row = leftHandLanes[laneIdx, 1];

                    notes.Add(new NoteData
                    {
                        time = beatTime,
                        col = col,
                        row = row,
                        color = BlockColor.Red,
                        cutDirection = GetSyncedDirection(
                            row, beatInMeasure, energyRatio, energyDelta,
                            isLeft: true, rng)
                    });
                    lastLeftTime = beatTime;
                    lastAnyTime = beatTime;
                    lastLeftLane = laneIdx;
                }

                // ── Right hand (staggered) ─────────────────────────
                if (rightReady && (choice == 1 || choice == 2))
                {
                    float offset = (choice == 2) ? minAnyNoteGap : 0f;
                    float rightTime = beatTime + offset;
                    if (rightTime > songLength - endPadding) continue;

                    // Beat position for the staggered time
                    int rightBeatInMeasure = Mathf.FloorToInt((rightTime / spb) % 4f);

                    int laneIdx;
                    do { laneIdx = rng.Next(0, rightHandLanes.GetLength(0)); }
                    while (laneIdx == lastRightLane && rightHandLanes.GetLength(0) > 1);

                    int col = rightHandLanes[laneIdx, 0];
                    int row = rightHandLanes[laneIdx, 1];

                    notes.Add(new NoteData
                    {
                        time = rightTime,
                        col = col,
                        row = row,
                        color = BlockColor.Blue,
                        cutDirection = GetSyncedDirection(
                            row, rightBeatInMeasure, energyRatio, energyDelta,
                            isLeft: false, rng)
                    });
                    lastRightTime = rightTime;
                    lastAnyTime = Mathf.Max(lastAnyTime, rightTime);
                    lastRightLane = laneIdx;
                }

                // ── Walls ──────────────────────────────────────────
                float clearWindow = spb * 1.5f; // 1.5 beats of clear space required

                bool playerLaneClear = true;
                foreach (var n in notes)
                {
                    if (Mathf.Abs(n.time - beatTime) < clearWindow)
                    {
                        playerLaneClear = false;
                        break;
                    }
                }

                if (playerLaneClear)
                {
                    for (int lookAhead = bi + 1;
                         lookAhead < beatList.Count &&
                         beatList[lookAhead].time - beatTime < clearWindow;
                         lookAhead++)
                    {
                        playerLaneClear = false;
                        break;
                    }
                }

                bool wallGapOk = beatTime - lastObsTime >= minWallGap;
                bool strongEnough = energyRatio >= wallEnergyMinRatio;

                if (playerLaneClear && wallGapOk && strongEnough &&
                    rng.NextDouble() < wallChance)
                {
                    bool col0Used = false, col3Used = false;
                    foreach (var n in notes)
                    {
                        if (Mathf.Abs(n.time - beatTime) < spb * 2f)
                        {
                            if (n.col == 0 || n.col == 1) col0Used = true;
                            if (n.col == 2 || n.col == 3) col3Used = true;
                        }
                    }

                    int wallCol;
                    if (col0Used && !col3Used) wallCol = 2; // right side free
                    else if (col3Used && !col0Used) wallCol = 0; // left side free
                    else wallCol = rng.NextDouble() < 0.5 ? 0 : 2;

                    obstacles.Add(new ObstacleData
                    {
                        time = beatTime,
                        col = wallCol,
                        width = 2,
                        duration = spb * 2f
                    });
                    lastObsTime = beatTime;

                    Debug.Log($"Wall at t={beatTime:F2}s side={wallCol} energyRatio={energyRatio:F2}");
                }
            }

            notes.Sort((a, b) => a.time.CompareTo(b.time));

            targetBeatMap.songName = songClip.name;
            targetBeatMap.songClip = songClip;
            targetBeatMap.bpm = bpm;
            targetBeatMap.noteJumpSpeed = noteJumpSpeed;
            targetBeatMap.notes = notes;
            targetBeatMap.obstacles = obstacles;

#if UNITY_EDITOR
            EditorUtility.SetDirty(targetBeatMap);
            AssetDatabase.SaveAssets();
            Debug.Log($"Generated {notes.Count} notes + {obstacles.Count} walls " +
                      $"from {beatList.Count} beats. " +
                      $"minSameHand={minSameHandGap:F2}s  minAny={minAnyNoteGap:F2}s");
#endif
        }

        // ── Beat-synced direction logic ─────────────────────────────
        CutDirection GetSyncedDirection(
            int row, int beatInMeasure, float energyRatio,
            float energyDelta, bool isLeft, System.Random rng)
        {
            // If sync is disabled or random roll fails, use row-based natural direction
            if (!syncDirectionsToBeats || rng.NextDouble() > directionSyncStrength)
                return NaturalDirection(row, rng);

            // ── Rule 1: Beat position in measure ─────────────────
            CutDirection measureDir = beatInMeasure switch
            {
                0 => CutDirection.Down,   // beat 1 — strongest, punch down
                1 => CutDirection.Up,     // beat 2 — lift up
                2 => CutDirection.Down,   // beat 3 — strong again
                _ => CutDirection.Up      // beat 4 — anticipation, lift
            };

            // ── Rule 2: Energy direction (rising vs falling) ──────
            CutDirection energyDir;
            if (Mathf.Abs(energyDelta) < 0.001f)
            {
                // Flat energy — use measure position
                energyDir = measureDir;
            }
            else
            {
                energyDir = energyDelta > 0 ? CutDirection.Up : CutDirection.Down;
            }

            // ── Rule 3: Hand mirroring ────────────────────────────
            if (beatInMeasure == 0 || beatInMeasure == 2) // strong beats
            {
                return isLeft ? CutDirection.Right : CutDirection.Left;
            }

            // ── Rule 4: Very high energy = dot block ─────────────
            if (energyRatio > 2.2f)
                return CutDirection.Any;

            // ── Blend energy direction with measure direction ─────
            if (energyDir == measureDir)
                return energyDir;
            else
                return measureDir;
        }

        CutDirection NaturalDirection(int row, System.Random rng)
        {
            CutDirection[] rowDirs = {
                CutDirection.Up,
                CutDirection.Any,
                CutDirection.Down
            };
            if (rng.NextDouble() < 0.7f) return rowDirs[row];
            return (CutDirection)rng.Next(0, 5);
        }
    }
}