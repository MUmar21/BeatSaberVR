using UnityEngine;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

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

        // ── Validation ───────────────────────────────────
        if (targetBeatMap == null)
        {
            Debug.LogError("Generator: Assign a BeatMapSO in the Target field first.");
            return;
        }
        if (songClip == null)
        {
            Debug.LogError("Generator: Assign a Song Clip first.");
            return;
        }

        // ── Setup ────────────────────────────────────────
        System.Random rng = new System.Random(42); // fixed seed = same map every time
        float songLength = songClip.length;
        float secondsPerBeat = 60f / bpm;
        float secondsPerStep = secondsPerBeat / subdivision;
        int totalSteps = Mathf.FloorToInt(songLength / secondsPerStep);

        List<NoteData> notes = new List<NoteData>();
        List<ObstacleData> obstacles = new List<ObstacleData>();

        int lastLeftCol = -1, lastRightCol = -1;
        int lastLeftRow = -1, lastRightRow = -1;
        int lastLeftDir = -1, lastRightDir = -1;
        float lastObsTime = -99f;

        // ── Generate notes ───────────────────────────────
        for (int step = 0; step < totalSteps; step++)
        {

            float time = step * secondsPerStep;

            // Respect padding
            if (time < startPadding) continue;
            if (time > songLength - endPadding) continue;

            // Left hand (red)
            if (rng.NextDouble() < noteDensity)
            {
                notes.Add(MakeNote(
                    time, BlockColor.Left,
                    ref lastLeftCol, ref lastLeftRow, ref lastLeftDir, rng));
            }

            // Right hand (blue) — stagger by half a step so hands don't always land together
            float rightTime = time + secondsPerStep * 0.5f;
            if (rightTime < songLength - endPadding && rng.NextDouble() < noteDensity)
            {
                notes.Add(MakeNote(
                    rightTime, BlockColor.Right,
                    ref lastRightCol, ref lastRightRow, ref lastRightDir, rng));
            }

            // Walls — roughly every 8 seconds, random side
            if (time - lastObsTime > 8f && rng.NextDouble() < 0.15f)
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

        // Sort by time (safety)
        notes.Sort((a, b) => a.time.CompareTo(b.time));

        // ── Write into the ScriptableObject ─────────────
        targetBeatMap.songName = songClip.name;
        targetBeatMap.songClip = songClip;
        targetBeatMap.bpm = bpm;
        targetBeatMap.noteJumpSpeed = noteJumpSpeed;
        targetBeatMap.notes = notes;
        targetBeatMap.obstacles = obstacles;

#if UNITY_EDITOR
        // Mark the asset dirty so Unity saves the changes to disk
        EditorUtility.SetDirty(targetBeatMap);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Generator: Wrote {notes.Count} notes + {obstacles.Count} obstacles " +
                  $"into '{targetBeatMap.name}'. Song: {songClip.name}, " +
                  $"BPM: {bpm}, Duration: {songLength:F1}s");
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