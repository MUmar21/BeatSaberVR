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
            BeatSaberVREvents.OnBlockSpawned?.Invoke(color);
        }

        public void SpawnWall(int col, int width, float zDepth)
        {
            WallBehavior wall = BlockPoolManager.Instance.GetWall();
            if (wall == null) return;

            float wallWidth = width * 0.6f;
            float leftEdge = -0.9f + col * 0.2f;
            float centerX = leftEdge + wallWidth * 0.5f;
            Vector3 pos = new Vector3(centerX, 1.4f, zDepth);

            wall.Setup(wallWidth, pos);
            wall.gameObject.SetActive(true);

            BeatSaberVREvents.OnBlockSpawned?.Invoke(BlockColor.Red); // Using Red as a placeholder since walls don't have a color.
        }
    }
}