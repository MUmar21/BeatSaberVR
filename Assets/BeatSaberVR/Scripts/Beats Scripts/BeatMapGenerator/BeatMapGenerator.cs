using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BeatSaberVR
{
    public enum BeatType { Block, Wall, Skip }

    public class BeatMapGenerator : MonoBehaviour
    {
        [Header("Target")]
        public BeatMapSO targetBeatMap;
        public AudioClip songClip;
        public float bpm = 128f;
        public float noteJumpSpeed = 10f;

        [Header("Beat Detection")]
        public int windowSize = 1024;
        [Range(1.1f, 3f)]
        public float beatThreshold = 1.35f;
        public float minBeatGap = 0.3f;

        [Header("Frequency Classification")]
        [Tooltip("ZCR below this = sustained/bass sound → wall candidate")]
        public float wallZcrMax = 0.08f;
        [Tooltip("ZCR above this = sharp/treble sound → block")]
        public float blockZcrMin = 0.05f;
        [Tooltip("How many consecutive windows energy stays high = sustained sound")]
        public int sustainWindowCount = 4;
        [Tooltip("Energy ratio required for a wall (walls need to be strong beats)")]
        public float wallEnergyMinRatio = 1.5f;

        [Header("Note Spacing")]
        public float minSameHandGap = 0.55f;
        public float minAnyNoteGap = 0.3f;
        [Range(0.3f, 1f)]
        public float noteDensity = 0.75f;

        [Header("Wall Spacing")]
        public float minWallGap = 5f;
        [Tooltip("Minimum clear seconds before and after a wall (no blocks)")]
        public float wallClearBefore = 1.0f;
        public float wallClearAfter = 0.8f;

        [Header("Cut Direction Sync")]
        public bool syncDirectionsToBeats = true;
        [Range(0f, 1f)]
        public float directionSyncStrength = 0.8f;

        [Header("Padding")]
        public float startPadding = 8f;
        public float endPadding = 3f;

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

            // ── Step 1: Extract mono samples ───────────────────────
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

            // ── Step 3: Zero Crossing Rate per window ──────────────
            float[] zcr = new float[numWindows];
            for (int w = 0; w < numWindows; w++)
            {
                int crossings = 0;
                int off = w * windowSize;
                int end = Mathf.Min(off + windowSize, monoLength) - 1;
                for (int i = off; i < end; i++)
                {
                    if ((mono[i] >= 0f) != (mono[i + 1] >= 0f))
                        crossings++;
                }
                zcr[w] = (float)crossings / windowSize;
            }

            // ── Step 4: Energy sustain per window ──────────────────
            float[] avgEnergy = new float[numWindows];
            int hist = Mathf.Max(1,
                           Mathf.RoundToInt((float)sampleRate / windowSize));

            for (int w = 0; w < numWindows; w++)
            {
                int from = Mathf.Max(0, w - hist);
                int to = Mathf.Min(numWindows - 1, w + hist);
                float s = 0f;
                for (int i = from; i <= to; i++) s += energy[i];
                avgEnergy[w] = s / (to - from + 1);
            }

            int[] sustainLength = new int[numWindows];
            for (int w = 0; w < numWindows; w++)
            {
                int count = 0;
                for (int fw = w;
                     fw < numWindows && energy[fw] >= avgEnergy[fw] * 1.1f;
                     fw++)
                    count++;
                sustainLength[w] = count;
            }

            // ── Step 5: Detect and classify beats ──────────────────
            var beatList = new List<(float time, float ratio,
                                     float delta, BeatType type)>();
            float lastBeat = -99f;

            int wallCount = 0, blockBeatCount = 0;

            for (int w = 1; w < numWindows - 1; w++)
            {
                float time = (float)(w * windowSize) / sampleRate;

                if (time < startPadding || time > songLength - endPadding) continue;

                bool isPeak = energy[w] > energy[w - 1] &&
                                   energy[w] > energy[w + 1];
                float ratio = avgEnergy[w] > 0f ?
                                   energy[w] / avgEnergy[w] : 0f;
                bool aboveThres = ratio >= beatThreshold;
                bool gapOk = time - lastBeat >= minBeatGap;

                if (!isPeak || !aboveThres || !gapOk) continue;

                float delta = energy[w] - energy[Mathf.Max(0, w - 2)];
                float windowZcr = zcr[w];
                int sustain = sustainLength[w];

                bool isWallCandidate =
                    windowZcr <= wallZcrMax &&
                    sustain >= sustainWindowCount &&
                    ratio >= wallEnergyMinRatio;

                bool isBlockCandidate =
                    windowZcr >= blockZcrMin ||
                    sustain < sustainWindowCount;

                BeatType type;
                if (isWallCandidate) { type = BeatType.Wall; wallCount++; }
                else if (isBlockCandidate) { type = BeatType.Block; blockBeatCount++; }
                else { type = BeatType.Skip; }

                beatList.Add((time, ratio, delta, type));
                lastBeat = time;
            }

            Debug.Log($"Beat classification: {blockBeatCount} block beats, " +
                      $"{wallCount} wall beats, " +
                      $"{beatList.Count} total in {songLength:F1}s");

            // ── Step 6: Convert beats to notes and obstacles ────────
            var rng = new System.Random(42);
            var notes = new List<NoteData>();
            var obstacles = new List<ObstacleData>();

            float lastLeftTime = -99f, lastRightTime = -99f, lastAnyTime = -99f;
            float lastObsTime = -99f;
            int lastLeftLane = -1, lastRightLane = -1;
            float spb = 60f / bpm;

            for (int bi = 0; bi < beatList.Count; bi++)
            {
                var (beatTime, energyRatio, energyDelta, beatType) = beatList[bi];

                // ── WALL beat ──────────────────────────────────────
                if (beatType == BeatType.Wall)
                {
                    bool wallGapOk = beatTime - lastObsTime >= minWallGap;
                    if (!wallGapOk) continue;

                    // Ensure no blocks too close before this wall
                    bool clearBefore = beatTime - lastAnyTime >= wallClearBefore;

                    // Ensure no upcoming block beats too close after
                    bool clearAfter = true;
                    for (int look = bi + 1; look < beatList.Count; look++)
                    {
                        float lookTime = beatList[look].time;
                        if (lookTime - beatTime > wallClearAfter) break;
                        if (beatList[look].type == BeatType.Block)
                        { clearAfter = false; break; }
                    }

                    if (!clearBefore || !clearAfter) continue;

                    bool wallLeft = lastRightTime > lastLeftTime;
                    int wallCol = wallLeft ? 0 : 2;

                    // Wall height — full height for normal walls
                    obstacles.Add(new ObstacleData
                    {
                        time = beatTime,
                        col = wallCol,
                        width = 1,
                        duration = spb * 2f   // 2 beats — visible approach time
                    });
                    lastObsTime = beatTime;

                    Debug.Log($"WALL → t={beatTime:F2}s  zcr={zcr[(int)(beatTime * sampleRate / windowSize)]:F3}  " +
                              $"sustain={sustainLength[(int)(beatTime * sampleRate / windowSize)]}  " +
                              $"ratio={energyRatio:F2}");
                    continue; // Don't also add blocks for wall beats
                }

                // ── BLOCK beat ─────────────────────────────────────
                if (beatType == BeatType.Skip) continue;
                if (rng.NextDouble() > noteDensity) continue;

                bool leftReady = (beatTime - lastLeftTime >= minSameHandGap) &&
                                  (beatTime - lastAnyTime >= minAnyNoteGap);
                bool rightReady = (beatTime - lastRightTime >= minSameHandGap) &&
                                  (beatTime - lastAnyTime >= minAnyNoteGap);

                if (!leftReady && !rightReady) continue;

                int beatInMeasure = Mathf.FloorToInt((beatTime / spb) % 4f);
                int choice = rng.Next(0, 3);

                // Left hand
                if (leftReady && (choice == 0 || choice == 2))
                {
                    int laneIdx;
                    do { laneIdx = rng.Next(0, leftHandLanes.GetLength(0)); }
                    while (laneIdx == lastLeftLane &&
                           leftHandLanes.GetLength(0) > 1);

                    notes.Add(new NoteData
                    {
                        time = beatTime,
                        col = leftHandLanes[laneIdx, 0],
                        row = leftHandLanes[laneIdx, 1],
                        color = BlockColor.Red,
                        cutDirection = GetSyncedDirection(
                            leftHandLanes[laneIdx, 1], beatInMeasure,
                            energyRatio, energyDelta, true, rng)
                    });
                    lastLeftTime = beatTime;
                    lastAnyTime = beatTime;
                    lastLeftLane = laneIdx;
                }

                // Right hand (staggered)
                float rightTime = beatTime + (choice == 2 ? minAnyNoteGap : 0f);
                if (rightReady && (choice == 1 || choice == 2) &&
                    rightTime < songLength - endPadding)
                {
                    int rightBeatInMeasure = Mathf.FloorToInt((rightTime / spb) % 4f);
                    int laneIdx;
                    do { laneIdx = rng.Next(0, rightHandLanes.GetLength(0)); }
                    while (laneIdx == lastRightLane &&
                           rightHandLanes.GetLength(0) > 1);

                    notes.Add(new NoteData
                    {
                        time = rightTime,
                        col = rightHandLanes[laneIdx, 0],
                        row = rightHandLanes[laneIdx, 1],
                        color = BlockColor.Blue,
                        cutDirection = GetSyncedDirection(
                            rightHandLanes[laneIdx, 1], rightBeatInMeasure,
                            energyRatio, energyDelta, false, rng)
                    });
                    lastRightTime = rightTime;
                    lastAnyTime = Mathf.Max(lastAnyTime, rightTime);
                    lastRightLane = laneIdx;
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
            Debug.Log($"GENERATED → {notes.Count} notes + " +
                      $"{obstacles.Count} walls from {beatList.Count} beats.");
#endif
        }

        CutDirection GetSyncedDirection(
            int row, int beatInMeasure, float energyRatio,
            float energyDelta, bool isLeft, System.Random rng)
        {
            if (!syncDirectionsToBeats ||
                rng.NextDouble() > directionSyncStrength)
                return NaturalDirection(row, rng);

            CutDirection measureDir = beatInMeasure switch
            {
                0 => CutDirection.Down,
                1 => CutDirection.Up,
                2 => CutDirection.Down,
                _ => CutDirection.Up
            };

            if (beatInMeasure == 0 || beatInMeasure == 2)
                return isLeft ? CutDirection.Right : CutDirection.Left;

            if (energyRatio > 2.2f) return CutDirection.Any;

            CutDirection energyDir = Mathf.Abs(energyDelta) < 0.001f
                ? measureDir
                : (energyDelta > 0 ? CutDirection.Up : CutDirection.Down);

            return energyDir == measureDir ? energyDir : measureDir;
        }

        CutDirection NaturalDirection(int row, System.Random rng)
        {
            CutDirection[] rowDirs = {
                CutDirection.Up, CutDirection.Any, CutDirection.Down
            };
            if (rng.NextDouble() < 0.7f) return rowDirs[row];
            return (CutDirection)rng.Next(0, 5);
        }
    }
}