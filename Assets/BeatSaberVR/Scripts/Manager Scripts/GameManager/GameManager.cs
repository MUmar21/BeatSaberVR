using UnityEngine;

namespace BeatSaberVR
{
    public enum BlockColor { Red, Blue }
    public enum CutDirection { Up, Down, Left, Right, Any }

    public class GameManager : Singleton<GameManager>
    {
        private int score = 0;
        private int combo = 0;

        [Header("For Testing")]
        [SerializeField] private bool noGameOver = false;
        public bool PlayGameOnStart = false;

        [Header("Energy Settings")]
        [SerializeField] private float maxEnergy = 100f;
        [SerializeField] private float startEnergy = 50f;
        [SerializeField] private float energyPerGoodHit = 4f;   // gain on hit
        [SerializeField] private float energyPerMiss = -8f;  // lose on miss
        [SerializeField] private float energyPerWallHit = -5f;  // lose on wall
        [SerializeField] private float gameOverThreshold = 0f;   // fail at 0

        private float currentEnergy;
        private bool isGameOver = false;

        public bool GameplayStarted { get; set; }

        private void OnEnable()
        {
            BeatSaberVREvents.OnGameStart += StartGame;
            BeatSaberVREvents.OnGameplayEnd += EndGameplay;
        }

        private void OnDisable()
        {
            BeatSaberVREvents.OnGameStart -= StartGame;
            BeatSaberVREvents.OnGameplayEnd -= EndGameplay;
        }

        public void AddScore(int points)
        {
            if (isGameOver) return;

            combo++;
            int total = points * GetComboMultiplier();
            score += total;
            currentEnergy = Mathf.Clamp(currentEnergy + energyPerGoodHit, 0f, maxEnergy);

            UIManager.Instance.UpdateScoreAndComboTexts(score, combo);
            UIManager.Instance.UpdateEnergyBar(currentEnergy / maxEnergy);
        }

        public void RegisterMiss()
        {
            if (isGameOver) return;

            combo = 0;
            currentEnergy = Mathf.Clamp(currentEnergy + energyPerMiss, 0f, maxEnergy);

            UIManager.Instance.UpdateScoreAndComboTexts(score, combo);
            UIManager.Instance.UpdateEnergyBar(currentEnergy / maxEnergy);

            CheckGameOver();
        }

        public void RegisterWallHit()
        {
            if (isGameOver) return;

            combo = 0;
            currentEnergy = Mathf.Clamp(currentEnergy + energyPerWallHit, 0f, maxEnergy);

            UIManager.Instance.UpdateScoreAndComboTexts(score, combo);
            UIManager.Instance.UpdateEnergyBar(currentEnergy / maxEnergy);

            CheckGameOver();
        }

        private int GetComboMultiplier()
        {
            if (combo >= 32) return 8;
            if (combo >= 16) return 4;
            if (combo >= 8) return 2;
            return 1;
        }

        private void CheckGameOver()
        {
            if (noGameOver) return;

            if (currentEnergy <= gameOverThreshold)
                TriggerGameOver();
        }

        private void TriggerGameOver()
        {
            if (isGameOver) return;
            isGameOver = true;
            GameplayStarted = false;

            Debug.Log($"GAME OVER — Final score: {score}  Combo: {combo}");

            BeatSaberVREvents.OnGameplayEnd?.Invoke();
            BeatSaberVREvents.OnGameOver?.Invoke(score);
        }

        private void StartGame()
        {
            ResetGameProps();
            GameplayStarted = true;
            UIManager.Instance.UpdateScoreAndComboTexts(score, combo);
            UIManager.Instance.UpdateEnergyBar(currentEnergy / maxEnergy);
        }

        private void EndGameplay()
        {
            GameplayStarted = false;
        }

        private void ResetGameProps()
        {
            score = 0;
            combo = 0;
            currentEnergy = startEnergy;
            isGameOver = false;
        }

        public int GetScore() => score;
        public float GetEnergy() => currentEnergy;
    }
}