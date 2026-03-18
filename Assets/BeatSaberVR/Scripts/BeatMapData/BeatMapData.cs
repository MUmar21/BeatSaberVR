using System;
using System.Collections.Generic;

[Serializable]
public class BeatMapNote
{
    public float time;
    public int lineIndex;    // column 0-3
    public int lineLayer;    // row 0-2
    public int type;         // 0=red, 1=blue
    public int cutDirection; // 0=up,1=down,2=left,3=right,4=any
}

[Serializable]
public class BeatMapObstacle
{
    public float time;
    public int lineIndex;
    public int width;
    public float duration;
}

[Serializable]
public class BeatMapData
{
    public string songName;
    public float bpm;
    public float noteJumpSpeed;
    public List<BeatMapNote> notes;
    public List<BeatMapObstacle> obstacles;
}