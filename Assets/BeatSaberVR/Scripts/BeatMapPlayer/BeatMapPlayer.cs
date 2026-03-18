using System.Collections;
using UnityEngine;

public class BeatMapPlayer : MonoBehaviour
{
    [Header("References")]
    public BeatMapSO beatMap;
    public AudioSource audioSource;
    public BlockSpawner blockSpawner;

    [Header("Timing")]
    public float spawnDistance = 25f;
    public float noteJumpSpeed = 10f;

    // How many seconds ahead to look for notes to spawn
    // Slightly larger than approachOffset to never miss a note
    private const float LOOKAHEAD = 0.1f;

    private float approachOffset;
    private int noteIndex = 0;   // cursor into beatMap.notes list
    private int obstacleIndex = 0;   // cursor into beatMap.obstacles list
    private bool isPlaying = false;

    void Start()
    {
        if (beatMap == null) { Debug.LogError("No BeatMap assigned!"); return; }

        approachOffset = spawnDistance / noteJumpSpeed;

        audioSource.clip = beatMap.songClip;
        StartCoroutine(StartWithDelay(0.1f));
    }

    IEnumerator StartWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.Play();
        isPlaying = true;
        Debug.Log($"Playing '{beatMap.songName}' — {beatMap.notes.Count} notes, " +
                  $"approachOffset={approachOffset:F2}s");
    }

    void Update()
    {
        if (!isPlaying) return;

        float songTime = audioSource.time;
        float spawnThreshold = songTime + approachOffset + LOOKAHEAD;

        // ── Notes (index walk — O(1) per frame, no Queue allocation) ──
        while (noteIndex < beatMap.notes.Count)
        {
            NoteData note = beatMap.notes[noteIndex];

            if (note.time - approachOffset <= songTime)
            {
                blockSpawner.SpawnBlock(note.col, note.row, note.color, note.cutDirection);
                noteIndex++;
            }
            else break; // list is sorted — nothing further is ready
        }

        // ── Obstacles ──
        while (obstacleIndex < beatMap.obstacles.Count)
        {
            ObstacleData obs = beatMap.obstacles[obstacleIndex];

            if (obs.time - approachOffset <= songTime)
            {
                blockSpawner.SpawnWall(obs.col, obs.width, spawnDistance);
                obstacleIndex++;
            }
            else break;
        }

        // ── End of song ──
        if (!audioSource.isPlaying &&
            noteIndex >= beatMap.notes.Count &&
            obstacleIndex >= beatMap.obstacles.Count)
        {
            isPlaying = false;
            Debug.Log("Song complete.");
        }
    }

    public void Pause() { audioSource.Pause(); isPlaying = false; }
    public void Resume() { audioSource.UnPause(); isPlaying = true; }
    public void Stop() { audioSource.Stop(); isPlaying = false; noteIndex = 0; obstacleIndex = 0; }
}