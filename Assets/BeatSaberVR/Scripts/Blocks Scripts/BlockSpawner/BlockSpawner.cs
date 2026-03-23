using UnityEngine;
namespace BeatSaberVR
{
    public class BlockSpawner : MonoBehaviour
    {
        public float spawnZ = 25f; // must match BeatMapPlayer.spawnDistance

        public void SpawnBlock(int col, int row, BlockColor color, CutDirection dir)
        {
            BlockBehavior block = BlockPoolManager.Instance.GetBlock(color);
            if (block == null) return;

            block.transform.position = LaneHelper.GridToWorld(col, row, spawnZ);
            block.transform.rotation = Quaternion.identity;
            block.blockColor = color;
            block.cutDirection = dir;
            block.gameObject.SetActive(true);
            block.SetDirectionPoint(dir);
        }

        public void SpawnWall(int col, int width, float zDepth)
        {
            WallBehavior wall = BlockPoolManager.Instance.GetWall();
            if (wall == null) return;

            float startX = -0.9f + col * 0.6f;
            float wallWidth = width * 0.6f;
            float centerX = startX + wallWidth / 2f - 0.3f;

            wall.transform.position = new Vector3(centerX, 1.1f, zDepth);
            wall.transform.localScale = new Vector3(wallWidth, wall.transform.localScale.y, wall.transform.localScale.z);
        }
    }
}