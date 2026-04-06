using UnityEngine;
namespace BeatSaberVR
{
    public static class LaneHelper
    {
        private static readonly float[] colX = { -0.3f, 0.3f };

        private static readonly float[] rowY = { 1.1f, 1.1f };

        public static Vector3 GridToWorld(int col, int row, float zDepth)
        {
            col = Mathf.Clamp(col, 0, 2);
            row = Mathf.Clamp(row, 0, 1);
            return new Vector3(colX[col], rowY[row], zDepth);
        }
    }
}