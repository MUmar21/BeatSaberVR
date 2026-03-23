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

        [Header("Difficulty")]
        [Range(0.1f, 1f)]
        public float noteDensity = 0.4f;
        public int subdivision = 2;   // 1=quarter, 2=eighth, 4=sixteenth

        [Header("Padding")]
        public float startPadding = 8f;  // must be >= approachOffset (spawnDist/jumpSpeed)
        public float endPadding = 3f;

        // Each pattern = sequence of (col, row, color, dir) for left then right hand
        // color: 0=Left/Red, 1=Right/Blue
        private static readonly int[,] patterns = new int[,]
        {
            // { leftCol, leftRow, leftDir,  rightCol, rightRow, rightDir }
            // dir: 0=up, 1=down, 2=left, 3=right, 4=any
            { 0, 1, 3,   3, 1, 2 },   // left-right sweep
            { 1, 1, 0,   2, 1, 0 },   // both up center
            { 0, 0, 0,   3, 0, 0 },   // both low corners up
            { 1, 2, 1,   2, 2, 1 },   // both high center down
            { 0, 1, 3,   2, 1, 3 },   // both sweep right
            { 1, 1, 2,   3, 1, 2 },   // both sweep left
            { 0, 0, 3,   3, 2, 2 },   // diagonal cross
            { 1, 0, 0,   2, 2, 1 },   // vertical opposites
            { 0, 2, 1,   3, 2, 1 },   // both high down
            { 1, 1, 4,   2, 1, 4 },   // dot blocks center
            { 0, 1, 0,   3, 1, 0 },   // wide up
            { 1, 0, 3,   2, 2, 2 },   // cross sweep
        };

        [ContextMenu("Generate Into ScriptableObject")]
        public void Generate()
        {
            if (targetBeatMap == null || songClip == null)
            {
                Debug.LogError("Assign BeatMapSO and AudioClip first."); return;
            }

            System.Random rng = new System.Random(42);
            float songLength = songClip.length;
            float secondsPerBeat = 60f / bpm;
            float secondsPerStep = secondsPerBeat / subdivision;
            int totalSteps = Mathf.FloorToInt(songLength / secondsPerStep);

            List<NoteData> notes = new List<NoteData>();
            List<ObstacleData> obstacles = new List<ObstacleData>();

            // Minimum gap between notes of same hand = 1 full beat
            float minGap = secondsPerBeat;
            float lastLeftTime = -99f;
            float lastRightTime = -99f;
            float lastObsTime = -99f;
            int lastPattern = -1;

            for (int step = 0; step < totalSteps; step++)
            {
                float time = step * secondsPerStep;

                if (time < startPadding) continue;
                if (time > songLength - endPadding) continue;

                bool leftReady = time - lastLeftTime >= minGap;
                bool rightReady = time - lastRightTime >= minGap;

                if (!leftReady && !rightReady) continue;

                // Roll for this beat
                if (rng.NextDouble() > noteDensity) continue;

                // Pick a pattern different from the last one
                int patternIdx;
                do { patternIdx = rng.Next(0, patterns.GetLength(0)); }
                while (patternIdx == lastPattern);
                lastPattern = patternIdx;

                // ── Left hand note ────────────────────────────
                if (leftReady)
                {
                    notes.Add(new NoteData
                    {
                        time = time,
                        col = patterns[patternIdx, 0],
                        row = patterns[patternIdx, 1],
                        color = BlockColor.Red,
                        cutDirection = (CutDirection)patterns[patternIdx, 2]
                    });
                    lastLeftTime = time;
                }

                // ── Right hand note — staggered by half step ──
                float rightTime = time + secondsPerStep * 0.5f;
                if (rightReady && rightTime < songLength - endPadding)
                {
                    notes.Add(new NoteData
                    {
                        time = rightTime,
                        col = patterns[patternIdx, 3],
                        row = patterns[patternIdx, 4],
                        color = BlockColor.Blue,
                        cutDirection = (CutDirection)patterns[patternIdx, 5]
                    });
                    lastRightTime = rightTime;
                }

                // ── Solo left-only note occasionally ─────────
                if (leftReady && rng.NextDouble() < 0.2f)
                {
                    float soloTime = time + secondsPerStep;
                    if (soloTime - lastLeftTime >= minGap &&
                        soloTime < songLength - endPadding)
                    {
                        int soloCol = rng.Next(0, 4);
                        int soloRow = rng.NextDouble() < 0.5 ? 1 : (rng.NextDouble() < 0.5 ? 0 : 2);
                        notes.Add(new NoteData
                        {
                            time = soloTime,
                            col = soloCol,
                            row = soloRow,
                            color = BlockColor.Red,
                            cutDirection = (CutDirection)rng.Next(0, 5)
                        });
                        lastLeftTime = soloTime;
                    }
                }

                // ── Walls (only when no blocks nearby) ───────
                bool blocksFiring = Mathf.Abs(lastLeftTime - time) < 0.2f ||
                                    Mathf.Abs(lastRightTime - time) < 0.2f;

                if (!blocksFiring && time - lastObsTime > 8f && rng.NextDouble() < 0.1f)
                {
                    obstacles.Add(new ObstacleData
                    {
                        time = time,
                        col = rng.NextDouble() < 0.5 ? 0 : 2,
                        width = 1,
                        duration = 0.5f
                    });
                    lastObsTime = time;
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
            Debug.Log($"Generated {notes.Count} notes + {obstacles.Count} walls. " +
                      $"Song: {songLength:F0}s at {bpm}BPM. startPadding={startPadding}s");
#endif
        }
    }
}