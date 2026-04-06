using System.Collections;
using UnityEngine;

namespace BeatSaberVR
{
    public class BeatMapPlayer : MonoBehaviour
    {
        [Header("References")]
        public BeatMapSO beatMap;
        public AudioSource audioSource;
        public BlockSpawner blockSpawner;

        [Header("Timing")]
        public float spawnDistance = 25f;
        public float noteJumpSpeed = 10f;

        private float approachOffset;
        private int noteIndex = 0;
        private int obstacleIndex = 0;
        private bool isPlaying = false;

        private void OnEnable()
        {
            BeatSaberVREvents.OnGameStart += StartPlaying;
        }

        private void OnDisable()
        {
            BeatSaberVREvents.OnGameStart -= StartPlaying;
        }

        private void StartPlaying()
        {
            if (beatMap == null) { Debug.LogError("No BeatMap assigned!"); return; }

            // ── CRITICAL: full reset before every play/replay ──────
            StopAllCoroutines();
            audioSource.Stop();

            noteIndex = 0;
            obstacleIndex = 0;
            isPlaying = false;

            approachOffset = spawnDistance / noteJumpSpeed;
            audioSource.clip = beatMap.songClip;

            // Return all active blocks to pool so scene is clean
            PoolManager.Instance.ReturnAllActive();

            StartCoroutine(StartWithDelay(0.1f));
        }

        private IEnumerator StartWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            audioSource.Play();
            isPlaying = true;
            Debug.Log($"Playing '{beatMap.songName}' — {beatMap.notes.Count} notes, " +
                      $"approachOffset={approachOffset:F2}s");
        }

        private void Update()
        {
            if (!GameManager.Instance.GameplayStarted) return;
            if (!isPlaying) return;

            float songTime = audioSource.time;

            // ── Notes ──────────────────────────────────────────────
            while (noteIndex < beatMap.notes.Count)
            {
                NoteData note = beatMap.notes[noteIndex];
                float spawnAt = note.time - approachOffset;

                if (spawnAt < 0f) { noteIndex++; continue; }

                if (spawnAt <= songTime)
                {
                    blockSpawner.SpawnBlock(note.col, note.row,
                                            note.color, note.cutDirection);
                    noteIndex++;
                }
                else break;
            }

            // ── Obstacles ──────────────────────────────────────────
            while (obstacleIndex < beatMap.obstacles.Count)
            {
                ObstacleData obs = beatMap.obstacles[obstacleIndex];
                float spawnAt = obs.time - approachOffset;

                if (spawnAt < 0f) { obstacleIndex++; continue; }

                if (spawnAt <= songTime)
                {
                    blockSpawner.SpawnWall(obs.col, obs.width, spawnDistance);
                    obstacleIndex++;
                }
                else break;
            }

            // ── End of song ────────────────────────────────────────
            //if (!audioSource.isPlaying &&
            //    noteIndex >= beatMap.notes.Count &&
            //    obstacleIndex >= beatMap.obstacles.Count)
            //{
            //    isPlaying = false;
            //    BeatSaberVREvents.OnGameplayEnd?.Invoke();
            //    Debug.Log("Song complete.");
            //}
        }

        public void Pause() { audioSource.Pause(); isPlaying = false; }
        public void Resume() { audioSource.UnPause(); isPlaying = true; }

        public void Stop()
        {
            audioSource.Stop();
            isPlaying = false;
            noteIndex = 0;
            obstacleIndex = 0;
        }
    }
}