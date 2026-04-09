using UnityEngine;

namespace BeatSaberVR
{
    [CreateAssetMenu(fileName = "NewChoice", menuName = "ScriptableObject/ChoiceData")]
    public class ChoiceData : ScriptableObject
    {
        public ChoiceDataEntry[] ChoiceEntries;
    }

    [System.Serializable]
    public class ChoiceDataEntry
    {
        [Header("Green Option (Positive Choice)")]
        public string greenText;
        [Range(0f, 1f)] public float greenHappinessDelta;
        [Range(0f, 1f)] public float greenStressDelta;
        [Range(0f, 1f)] public float greenFinanceDelta;
        [Header("Red Option (Negative Choice)")]
        public string redText;
        [Range(0f, 1f)] public float redHappinessDelta;
        [Range(0f, 1f)] public float redStressDelta;
        [Range(0f, 1f)] public float redFinanceDelta;
    }
}