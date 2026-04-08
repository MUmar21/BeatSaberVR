using System.Collections;
using UnityEngine;

namespace BeatSaberVR
{
    public enum BlockColor { Red, Blue } // red = left, blue = right
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

        private bool isGameOver = false;
        private int score = 0;

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
            isGameOver = false;
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
                    TriggerGameOver();
                    yield break;
                }
                yield return null;
            }
            timerCoroutine = null;
        }

        private void AddScore(int points)
        {
            if (isGameOver) return;

            score += points;
            UIManager.Instance.UpdateScoreAndComboTexts(score);
        }

        private void TriggerGameOver()
        {
            if (isGameOver) return;

            isGameOver = true;
            GameplayStarted = false;
            BeatSaberVREvents.OnGameplayEnd?.Invoke();
            //BeatSaberVREvents.OnGameOver?.Invoke(score);
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
    }
}