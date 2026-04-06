using UnityEngine;

namespace BeatSaberVR
{
    [CreateAssetMenu(fileName = "NewChoice", menuName = "FinancialGame/ChoiceData")]
    public class ChoiceData : ScriptableObject
    {
        [Header("Green Option (Positive Choice)")]
        public string greenText;
        [Range(-1f, 1f)] public float greenHappinessDelta;   // e.g. +0.2 means +20%
        [Range(-1f, 1f)] public float greenStressDelta;      // e.g. -0.1 means -10%
        [Range(-1f, 1f)] public float greenFinanceDelta;     // e.g. +0.15 means +15%

        [Header("Red Option (Negative Choice)")]
        public string redText;
        [Range(-1f, 1f)] public float redHappinessDelta;     // e.g. -0.2 means -20%
        [Range(-1f, 1f)] public float redStressDelta;        // e.g. +0.25 means +25%
        [Range(-1f, 1f)] public float redFinanceDelta;       // e.g. -0.3 means -30%
    }
}