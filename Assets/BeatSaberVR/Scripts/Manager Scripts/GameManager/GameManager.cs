using System.Collections;
using UnityEngine;

namespace BeatSaberVR
{
    public enum BlockColor { Red, Blue } // red = right, blue = left
    public enum CutDirection { Up, Down, Left, Right, Any }

    public class GameManager : Singleton<GameManager>
    {
        [Header("For Testing")]
        public bool PlayGameOnStart = false;

        [Header("Timer")]
        [SerializeField] private float levelTimeLimit = 120f; // seconds
        [SerializeField] private bool useLevelTimer = true;
        private float timer;
        private Coroutine timerCoroutine;

        private int score = 0;

        private bool blocksAreActive;
        private bool timesUp;

        public bool GameOver { get; set; }
        public bool GameplayStarted { get; set; }

        private void OnEnable()
        {
            BeatSaberVREvents.OnGameStart += StartGame;
            BeatSaberVREvents.OnTriggerGameOver += TriggerGameOver;
            BeatSaberVREvents.OnAddScore += AddScore;
        }

        private void OnDisable()
        {
            BeatSaberVREvents.OnGameStart -= StartGame;
            BeatSaberVREvents.OnTriggerGameOver -= TriggerGameOver;
            BeatSaberVREvents.OnAddScore -= AddScore;
        }

        private void StartGame()
        {
            ResetGameProps();
            GameplayStarted = true;

            if (useLevelTimer)
            {
                timerCoroutine = StartCoroutine(Timer());
            }
        }

        private void ResetGameProps()
        {
            StopTimer();
            score = 0;
            timer = 0f;
            timesUp = false;
            GameOver = false;
            UIManager.Instance.UpdateScoreAndComboTexts(score);
        }

        private IEnumerator Timer()
        {
            while (GameplayStarted && useLevelTimer)
            {
                timer += Time.unscaledDeltaTime;
                UIManager.Instance.UpdateTimerText(levelTimeLimit - timer);

                if (timer >= levelTimeLimit)
                {
                    timesUp = true;
                    if (!blocksAreActive) TriggerGameOver();
                    yield break;
                }
                yield return null;
            }
            timerCoroutine = null;
        }

        private void AddScore(int points)
        {
            if (GameOver) return;

            score += points;
            score = (score < 0) ? 0 : score;
            UIManager.Instance.UpdateScoreAndComboTexts(score);
        }

        private void TriggerGameOver()
        {
            if (GameOver) return;

            GameOver = true;
            GameplayStarted = false;
            BeatSaberVREvents.OnGameplayEnd?.Invoke();
        }

        private void StopTimer()
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }
        }

        public int GetScore() => score;

        public void BlocksAreActive(bool active)
        {
            blocksAreActive = active;

            if (timesUp && !blocksAreActive)
            {
                TriggerGameOver();
            }
        }
    }
}