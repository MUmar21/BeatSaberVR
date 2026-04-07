using System.Collections;
using UnityEngine;
namespace BeatSaberVR
{
    public class BlockSpawner : MonoBehaviour
    {
        [SerializeField] private ChoiceData[] choiceDatas;
        [SerializeField] private float spawnZ = 25f; // must match BeatMapPlayer.spawnDistance
        [SerializeField] private float minSpawnDelay = 1f;
        [SerializeField] private float maxSpawnDelay = 5f;
        [SerializeField] private int numberOfOptionToSpawn = 2;

        private int currentChoiceIndex = 0;
        private float spawnDelay;
        private Coroutine spawningCoroutine;

        private void OnEnable()
        {
            BeatSaberVREvents.OnGameStart += OnStart;
            BeatSaberVREvents.OnGameplayEnd += StopSpawning;
        }

        private void OnDisable()
        {
            BeatSaberVREvents.OnGameStart -= OnStart;
            BeatSaberVREvents.OnGameplayEnd -= StopSpawning;
        }

        private void OnStart()
        {
            StopSpawning();
            spawningCoroutine = StartCoroutine(StartSpawning());
        }

        private IEnumerator StartSpawning()
        {
            while (GameManager.Instance.GameplayStarted && currentChoiceIndex < choiceDatas.Length)
            {
                spawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
                yield return new WaitForSeconds(spawnDelay);

                for (int i = 0; i < numberOfOptionToSpawn; i++)
                {
                    int randomCutDir = Random.Range(0, 4);
                    CutDirection cutDirection = (CutDirection)randomCutDir;
                    BlockColor color = (i == 0) ? BlockColor.Red : BlockColor.Blue;
                    SpawnBlock(i, 0, color, cutDirection);
                }

                currentChoiceIndex++;
            }

            BeatSaberVREvents.OnTriggerGameOver?.Invoke();
            spawningCoroutine = null;
        }

        private void StopSpawning()
        {
            if (spawningCoroutine != null)
            {
                StopCoroutine(spawningCoroutine);
                spawningCoroutine = null;
            }
        }

        public void SpawnBlock(int col, int row, BlockColor color, CutDirection dir)
        {
            BlockBehavior block = PoolManager.Instance.GetBlock(color);
            if (block == null) return;

            block.transform.position = LaneHelper.GridToWorld(col, row, spawnZ);
            block.transform.rotation = Quaternion.identity;
            block.blockColor = color;
            block.cutDirection = dir;
            block.gameObject.SetActive(true);
            block.SetDirectionPoint(dir);

            block.choiceData = choiceDatas[currentChoiceIndex];
            block.choiceText.text = (color == BlockColor.Blue) ? choiceDatas[currentChoiceIndex].greenText : choiceDatas[currentChoiceIndex].redText;

            BeatSaberVREvents.OnBlockSpawned?.Invoke(color);
        }

        public void SpawnWall(int col, int width, float zDepth)
        {
            WallBehavior wall = PoolManager.Instance.GetWall();
            if (wall == null) return;

            float wallWidth = width * 0.7f;
            float leftEdge = -0.9f + col * 0.2f;
            float centerX = leftEdge + wallWidth * 0.5f;
            Vector3 pos = new Vector3(centerX, 1.8f, zDepth);

            wall.Setup(wallWidth, pos);
            wall.gameObject.SetActive(true);

            BeatSaberVREvents.OnBlockSpawned?.Invoke(BlockColor.Red); // Using Red as a placeholder since walls don't have a color.
        }
    }
}