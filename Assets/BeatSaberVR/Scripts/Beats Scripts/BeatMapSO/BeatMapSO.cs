using System.Collections.Generic;
using UnityEngine;
namespace BeatSaberVR
{
    [CreateAssetMenu(fileName = "NewBeatMap", menuName = "Beat Saber/Beat Map")]
    public class BeatMapSO : ScriptableObject
    {
        public string songName;
        public AudioClip songClip;
        public float bpm;
        public float noteJumpSpeed = 10f;
        public List<NoteData> notes;
        public List<ObstacleData> obstacles;
    }
    [System.Serializable]
    public class NoteData
    {
        public float time;
        public int col;           // 0-3
        public int row;           // 0-2
        public BlockColor color;
        public CutDirection cutDirection;
    }

    [System.Serializable]
    public class ObstacleData
    {
        public float time;
        public int col;
        public int width;
        public float duration;
    }
}
