using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    public GameObject redBlockPrefab;
    public GameObject blueBlockPrefab;
    public GameObject wallPrefab;

    public float spawnZ = 25f; // must match BeatMapPlayer.spawnDistance

    public void SpawnBlock(int col, int row, BlockColor color, CutDirection dir)
    {
        GameObject prefab = (color == BlockColor.Left) ? redBlockPrefab : blueBlockPrefab;

        if (prefab == null)
        {
            Debug.LogError("BlockSpawner: prefab is null! Check Inspector references.");
            return;
        }

        Vector3 spawnPos = LaneHelper.GridToWorld(col, row, spawnZ);
        GameObject block = Instantiate(prefab, spawnPos, Quaternion.identity);

        BlockBehavior bb = block.GetComponent<BlockBehavior>();
        if (bb != null)
        {
            bb.blockColor = color;
            bb.cutDirection = dir;
            bb.SetDirectionPoint(dir);
        }

        Debug.Log($"Spawned {color} block at col={col} row={row} → world {spawnPos}");
    }

    public void SpawnWall(int col, int width, float zDepth)
    {
        if (wallPrefab == null) return;

        // Wall starts at the leftmost column it covers
        float startX = -0.9f + col * 0.6f;
        float wallWidth = width * 0.6f;
        float centerX = startX + wallWidth / 2f - 0.3f;

        Vector3 pos = new Vector3(centerX, 1.1f, zDepth);
        GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity);
        wall.transform.localScale = new Vector3(wallWidth, wall.transform.localScale.y, wall.transform.localScale.z);
    }
}