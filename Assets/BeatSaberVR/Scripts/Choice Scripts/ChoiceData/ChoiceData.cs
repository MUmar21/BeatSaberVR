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

        [ContextMenu("Populate Random Choice")]
        public void PopulateRandomChoice()
        {
            // Define various testing scenarios
            var templates = new[]
            {
                 new {
                    gT = "Save Money", gH = 0.3f, gS = 0.1f, gF = 0.3f,
                    rT = "Buy Luxury Car", rH = 0.5f, rS = 0.2f, rF = -0.4f
                },
                  new {
                    gT = "Build Emergency Fund", gH = 0.2f, gS = -0.1f, gF = 0.4f,
                    rT = "Spend on Luxury Watch", rH = 0.3f, rS = 0.4f, rF = -0.1f
                },
                  new {
                    gT = "Invest for Future", gH = 0.2f, gS = -0.1f, gF = 0.4f,
                    rT = "Take High Interest Loan", rH = -0.2f, rS = 0.7f, rF = 0.3f
                },
                new {
                    gT = "Invest in Stocks", gH = 0.2f, gS = 0.3f, gF = 0.6f,
                    rT = "Keep Cash in Mattress", rH = 0.1f, rS = -0.2f, rF = -0.1f
                },
                new {
                    gT = "Buy Health Insurance", gH = 0.4f, gS = -0.5f, gF = -0.3f,
                    rT = "Take the Risk", rH = 0.1f, rS = 0.6f, rF = 0.2f
                },
                new {
                    gT = "Pay Off Credit Card", gH = 0.5f, gS = -0.6f, gF = 0.1f,
                    rT = "Minimum Payment Only", rH = 0.2f, rS = 0.4f, rF = -0.4f
                },
                new {
                    gT = "Professional Course", gH = 0.3f, gS = 0.4f, gF = 0.7f,
                    rT = "Weekend Party", rH = 0.8f, rS = -0.4f, rF = -0.3f
                },
                new {
                    gT = "Start Side Hustle", gH = 0.1f, gS = 0.7f, gF = 0.5f,
                    rT = "Watch Netflix", rH = 0.4f, rS = -0.6f, rF = 0.0f
                }
            };

            var choice = templates[Random.Range(0, templates.Length)];

            // Apply the template data
            greenText = choice.gT;
            greenHappinessDelta = choice.gH;
            greenStressDelta = choice.gS;
            greenFinanceDelta = choice.gF;

            redText = choice.rT;
            redHappinessDelta = choice.rH;
            redStressDelta = choice.rS;
            redFinanceDelta = choice.rF;

            // Ensure the Inspector updates visually
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}