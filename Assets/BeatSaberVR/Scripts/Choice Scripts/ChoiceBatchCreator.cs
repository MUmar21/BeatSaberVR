using System.IO;
using UnityEditor;
using UnityEngine;

namespace BeatSaberVR
{
    public class ChoiceBatchCreator
    {
        [MenuItem("Tools/Financial Game/Generate 10 Random Choices")]
        public static void GenerateBatch()
        {
            string path = "Assets/_ChoiceData"; // Adjust folder path

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            for (int i = 0; i < 10; i++)
            {
                ChoiceData newChoice = ScriptableObject.CreateInstance<ChoiceData>();
                newChoice.PopulateRandomChoice();

                string fileName = $"{path}/Choice_{i}.asset";
                AssetDatabase.CreateAsset(newChoice, fileName);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Successfully generated 10 random financial choices!");
        }
    }
}