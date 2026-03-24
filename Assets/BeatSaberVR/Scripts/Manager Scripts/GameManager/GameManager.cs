using UnityEngine;

namespace BeatSaberVR
{
    public enum BlockColor { Red, Blue }   // Left = Red, Right = Blue
    public enum CutDirection { Up, Down, Left, Right, Any }

    public class GameManager : Singleton<GameManager>
    {
        private int score = 0;
        private int combo = 0;


        [SerializeField] private GameObject completeXROriginSetUpHandsVariant;

        public bool GameplayStarted { get; set; }

        private void OnEnable()
        {
            //completeXROriginSetUpHandsVariant.SetActive(false);

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
            combo++;
            int total = points * combo; // combo multiplier
            score += total;
            UIManager.Instance.UpdateScoreAndComboTexts(score, combo);
        }

        public void RegisterMiss()
        {
            combo = 0; // reset combo on miss
            UIManager.Instance.UpdateScoreAndComboTexts(score, combo);
        }

        public void RegisterWallHit()
        {
            RegisterMiss();
            UIManager.Instance.OnWallHit();
        }

        private void StartGame()
        {
            ResetGameProps();
            UIManager.Instance.UpdateScoreAndComboTexts(score, combo);
            GameplayStarted = true;
            //completeXROriginSetUpHandsVariant.SetActive(true);
        }

        private void EndGameplay()
        {
            GameplayStarted = false;
           // completeXROriginSetUpHandsVariant.SetActive(false);
        }

        private void ResetGameProps()
        {
            score = 0;
            combo = 0;
        }

    }
}