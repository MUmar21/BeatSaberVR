using UnityEngine;
namespace BeatSaberVR
{
    public static class LaneHelper
    {
        // 4 columns: spaced 0.6 units apart, centered at x=0
        // col 0 = far left, col 3 = far right
        private static readonly float[] colX = { -0.15f, -0.3f, 0.3f, 0.15f };

        // 3 rows: bottom to top
        private static readonly float[] rowY = { 0.79f, 0.85f, 0.98f };

        public static Vector3 GridToWorld(int col, int row, float zDepth)
        {
            col = Mathf.Clamp(col, 0, 3);
            row = Mathf.Clamp(row, 0, 2);
            return new Vector3(colX[col], rowY[row], zDepth);
        }
    }
}