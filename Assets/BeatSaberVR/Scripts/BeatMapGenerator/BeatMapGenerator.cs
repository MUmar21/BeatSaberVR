using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
#endif
namespace BeatSaberVR
{
    public class BeatMapGenerator : MonoBehaviour
    {
        [Header("Target")]
        public BeatMapSO targetBeatMap;      // drag your SO asset here

        [Header("Song Settings")]
        public AudioClip songClip;           // drag your audio clip here
        public float bpm = 128f;
        public float noteJumpSpeed = 10f;

        [Header("Difficulty")]
        [Range(0.1f, 1f)]
        public float noteDensity = 0.5f;
        public int subdivision = 2;          // 1=quarter, 2=eighth, 4=sixteenth

        [Header("Padding")]
        public float startPadding = 2f;      // seconds of silence before first note
        public float endPadding = 2f;      // seconds of silence before song ends

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

            int lastLeftCol = -1, lastRightCol = -1;
            int lastLeftRow = -1, lastRightRow = -1;
            int lastLeftDir = -1, lastRightDir = -1;
            float lastLeftTime = -99f, lastRightTime = -99f;
            float lastObsTime = -99f;

            // ── KEY: minimum time gap between same-hand notes ──────────
            // At 128 BPM this is ~0.47s = one full beat gap minimum
            float minGapSameHand = secondsPerBeat * 1.0f;

            // Also prevent BOTH hands spawning at same exact time too often
            float lastBothHandsTime = -99f;

            for (int step = 0; step < totalSteps; step++)
            {
                float time = step * secondsPerStep;

                if (time < startPadding || time > songLength - endPadding) continue;

                bool leftReady = time - lastLeftTime >= minGapSameHand;
                bool rightReady = time - lastRightTime >= minGapSameHand;

                // ── Left hand (red) ─────────────────────────────────────
                if (leftReady && rng.NextDouble() < noteDensity)
                {
                    notes.Add(MakeNote(time, BlockColor.Left,
                        ref lastLeftCol, ref lastLeftRow, ref lastLeftDir, rng));
                    lastLeftTime = time;
                }

                // ── Right hand (blue) — stagger half step ────────────────
                // Never spawn right at exact same time as left just fired
                float rightTime = time + secondsPerStep * 0.5f;
                bool justFiredLeft = Mathf.Abs(lastLeftTime - rightTime) < secondsPerStep * 0.3f;

                if (rightReady && !justFiredLeft &&
                    rightTime < songLength - endPadding &&
                    rng.NextDouble() < noteDensity)
                {

                    notes.Add(MakeNote(rightTime, BlockColor.Right,
                        ref lastRightCol, ref lastRightRow, ref lastRightDir, rng));
                    lastRightTime = rightTime;
                }

                // ── Walls — only when no blocks firing ──────────────────
                bool blocksFiring = Mathf.Abs(lastLeftTime - time) < 0.1f ||
                                    Mathf.Abs(lastRightTime - time) < 0.1f;

                if (!blocksFiring && time - lastObsTime > 8f && rng.NextDouble() < 0.12f)
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
            UnityEditor.EditorUtility.SetDirty(targetBeatMap);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"Generated {notes.Count} notes — min gap {minGapSameHand:F2}s per hand");
#endif
        }

        // ── Helpers ──────────────────────────────────────────
        NoteData MakeNote(float time, BlockColor color,
                          ref int lastCol, ref int lastRow, ref int lastDir,
                          System.Random rng)
        {

            // Column — avoid repeating same column twice in a row
            int col;
            do { col = rng.Next(0, 4); } while (col == lastCol);

            // Row — weighted toward middle
            int row;
            double r = rng.NextDouble();
            if (r < 0.25) row = 0; // bottom  25%
            else if (r < 0.75) row = 1; // middle  50%
            else row = 2; // top     25%

            // Direction — avoid repeating same direction twice in a row
            int dir;
            do { dir = rng.Next(0, 5); } while (dir == lastDir);
            // 0=up 1=down 2=left 3=right 4=any(dot)

            lastCol = col;
            lastRow = row;
            lastDir = dir;

            return new NoteData
            {
                time = time,
                col = col,
                row = row,
                color = color,
                cutDirection = (CutDirection)dir
            };
        }
    }
}