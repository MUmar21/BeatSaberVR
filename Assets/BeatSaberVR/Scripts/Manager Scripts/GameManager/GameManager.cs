using System.Collections;
using UnityEngine;

namespace BeatSaberVR
{
    public enum BlockColor { Red, Blue } // red = left, blue = right
    public enum CutDirection { Up, Down, Left, Right, Any }

    public class GameManager : Singleton<GameManager>
    {
        private int score = 0;
        //private int combo = 0;

        [Header("For Testing")]
        [SerializeField] private bool noGameOver = false;
        public bool PlayGameOnStart = false;

        //[Header("Energy Settings")]
        //[SerializeField] private float maxEnergy = 100f;
        //[SerializeField] private float startEnergy = 50f;
        //[SerializeField] private float energyPerGoodHit = 4f;   // gain on hit
        //[SerializeField] private float energyPerMiss = -8f;  // lose on miss
        //[SerializeField] private float energyPerWallHit = -5f;  // lose on wall
        //[SerializeField] private float gameOverThreshold = 0f;   // fail at 0

        [Header("Timer")]
        [SerializeField] private float levelTimeLimit = 120f; // seconds
        [SerializeField] private bool useLevelTimer = true;
        private float timer;
        private Coroutine timerCoroutine;

        private float currentEnergy;
        private bool isGameOver = false;

        public bool GameplayStarted { get; set; }

        private void OnEnable()
        {
            BeatSaberVREvents.OnGameStart += StartGame;
            BeatSaberVREvents.OnGameplayEnd += EndGameplay;
            BeatSaberVREvents.OnTriggerGameOver += TriggerGameOver;
        }

        private void OnDisable()
        {
            BeatSaberVREvents.OnGameStart -= StartGame;
            BeatSaberVREvents.OnGameplayEnd -= EndGameplay;
            BeatSaberVREvents.OnTriggerGameOver -= TriggerGameOver;
        }

        private void StartGame()
        {
            ResetGameProps();
            GameplayStarted = true;
            UIManager.Instance.UpdateScoreAndComboTexts(score);
            //UIManager.Instance.UpdateEnergyBar(currentEnergy / maxEnergy);

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
            //currentEnergy = startEnergy;
            isGameOver = false;
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

        public void AddScore(int points)
        {
            if (isGameOver) return;

            //int total = points * GetComboMultiplier();
            //score += total;

            UIManager.Instance.UpdateScoreAndComboTexts(score);
            //UIManager.Instance.UpdateEnergyBar(currentEnergy / maxEnergy);
        }

        public void RegisterMiss()
        {
            //if (isGameOver) return;

            ////combo = 0;
            ////currentEnergy = Mathf.Clamp(currentEnergy + energyPerMiss, 0f, maxEnergy);

            //UIManager.Instance.UpdateScoreAndComboTexts(score);
            ////UIManager.Instance.UpdateEnergyBar(currentEnergy / maxEnergy);

            //CheckGameOver();
        }

        public void RegisterWallHit()
        {
            if (isGameOver) return;

            //combo = 0;
            //currentEnergy = Mathf.Clamp(currentEnergy + energyPerWallHit, 0f, maxEnergy);

            UIManager.Instance.UpdateScoreAndComboTexts(score);
            //UIManager.Instance.UpdateEnergyBar(currentEnergy / maxEnergy);
            BeatSaberVREvents.OnWallHit?.Invoke();

            CheckGameOver();
        }

        //private int GetComboMultiplier()
        //{
        //    if (combo >= 32) return 8;
        //    if (combo >= 16) return 4;
        //    if (combo >= 8) return 2;
        //    return 1;
        //}

        private void CheckGameOver()
        {
            if (noGameOver) return;

            //if (currentEnergy <= gameOverThreshold)
            //TriggerGameOver();
        }

        private void TriggerGameOver()
        {
            if (isGameOver) return;
            isGameOver = true;
            GameplayStarted = false;

            BeatSaberVREvents.OnGameplayEnd?.Invoke();
            BeatSaberVREvents.OnGameOver?.Invoke(score);
        }

        private void EndGameplay()
        {
            GameplayStarted = false;
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
        public float GetEnergy() => currentEnergy;

    }
}