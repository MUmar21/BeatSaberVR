using System;
using UnityEngine;

namespace BeatSaberVR
{
    public class PlayerFinanceManager : Singleton<PlayerFinanceManager>
    {
        [Header("Starting Values (0–1)")]
        [Range(0f, 1f)][SerializeField] private float startingHappiness = 0.5f;
        [Range(0f, 1f)][SerializeField] private float startingStress = 0.3f;
        [Range(0f, 1f)][SerializeField] private float startingBalance = 0.5f;

        public float CurrentHappiness { get; private set; }
        public float CurrentStress { get; private set; }
        public float CurrentBalance { get; private set; }

        public override void Awake()
        {
            base.Awake();
            ResetStats();
        }

        public void ResetStats()
        {
            CurrentHappiness = startingHappiness;
            CurrentStress = startingStress;
            CurrentBalance = startingBalance;
        }
        private void Start()
        {
            NotifyUI();
        }

        public void ProcessChoice(ChoiceData data, BlockColor blockColor)
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
        }

        private void ApplyDeltas(float happinessDelta, float stressDelta, float financeDelta)
        {
            CurrentHappiness = Mathf.Clamp01(CurrentHappiness + happinessDelta);
            CurrentStress = Mathf.Clamp01(CurrentStress + stressDelta);
            CurrentBalance = Mathf.Clamp01(CurrentBalance + financeDelta);

            Debug.Log($"[Finance] H:{CurrentHappiness:P0}  S:{CurrentStress:P0}  B:{CurrentBalance:P0}");
            NotifyUI();
        }

        private void NotifyUI()
        {
            UIManager.Instance.UpdateFinanceRatesFill(CurrentHappiness, CurrentStress, CurrentBalance);
        }
    }
}