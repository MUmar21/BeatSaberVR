using System;
using UnityEngine;

namespace BeatSaberVR
{
    public class PlayerFinanceManager : Singleton<PlayerFinanceManager>
    {
        [SerializeField] public ChoiceData choiceDatas;

        [Header("Starting Values (0–1)")]
        [Range(0f, 1f)][SerializeField] private float startingHappiness = 0.5f;
        [Range(0f, 1f)][SerializeField] private float startingStress = 0.3f;
        [Range(0f, 1f)][SerializeField] private float startingBalance = 0.5f;
        [SerializeField] private int baseScore = 10;

        public float CurrentHappiness { get; private set; }
        public float CurrentStress { get; private set; }
        public float CurrentBalance { get; private set; }

        private float previousStress;

        public override void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            BeatSaberVREvents.OnGameStart += OnStart;
        }

        private void OnDisable()
        {
            BeatSaberVREvents.OnGameStart -= OnStart;
        }

        private void OnStart()
        {
            ResetStats();
            NotifyUI();
        }

        public void ResetStats()
        {
            CurrentHappiness = startingHappiness;
            CurrentStress = startingStress;
            CurrentBalance = startingBalance;
        }

        public void ProcessChoice(ChoiceDataEntry data, BlockColor blockColor)
        {
            if (blockColor == BlockColor.Blue)
            {
                ApplyDeltas(data.greenHappinessDelta,
                            data.greenStressDelta,
                            data.greenFinanceDelta);
            }
            else
            {
                ApplyDeltas(data.redHappinessDelta,
                            data.redStressDelta,
                            data.redFinanceDelta);
            }

            BeatSaberVREvents.OnChoiceMade?.Invoke();
        }

        private void ApplyDeltas(float happinessDelta, float stressDelta, float financeDelta)
        {
            CurrentHappiness = Mathf.Clamp01(happinessDelta);
            CurrentStress = Mathf.Clamp01(stressDelta);
            CurrentBalance = Mathf.Clamp01(financeDelta);

            Debug.Log($"[Finance] H:{CurrentHappiness:P0} S:{CurrentStress:P0} B:{CurrentBalance:P0}");

            // Happiness and Balance are rewards; Stress is a heavy penalty.
            float weightHappiness = 0.5f;
            float weightFinance = 0.5f;
            float weightStress = -0.4f;
            float performanceFactor = (CurrentHappiness * weightHappiness) +
                                      (CurrentBalance * weightFinance) +
                                      (CurrentStress * weightStress);

            performanceFactor = Math.Max(0, performanceFactor);
            int finalPoints = Mathf.RoundToInt(baseScore + (performanceFactor * 100f));

            BeatSaberVREvents.OnAddScore?.Invoke(finalPoints);
            NotifyUI();
        }

        private void NotifyUI()
        {
            UIManager.Instance.UpdateFinanceRatesFill(CurrentHappiness, CurrentStress, CurrentBalance);
        }
    }
}