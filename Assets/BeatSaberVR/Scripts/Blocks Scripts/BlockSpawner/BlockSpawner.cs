using System.Collections;
using UnityEngine;
namespace BeatSaberVR
{
    public class BlockSpawner : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private PlayerFinanceManager playerFinanceManager;
        [SerializeField] private float spawnZ = 25f;
        [SerializeField] private float delayBetweenChoices = 2.0f;

        private int currentChoiceIndex = 0;
        private Coroutine spawnCoroutine;

        private void Start()
        {
            if (playerFinanceManager == null)
            {
                playerFinanceManager = PlayerFinanceManager.Instance;
                if (playerFinanceManager == null)
                    Debug.LogError("PlayerFinanceManager instance not found!");
            }
        }

        private void OnEnable()
        {
            BeatSaberVREvents.OnGameStart += OnStart;
            BeatSaberVREvents.OnChoiceMade += OnChoiceMade;
        }

        private void OnDisable()
        {
            BeatSaberVREvents.OnGameStart -= OnStart;
            BeatSaberVREvents.OnChoiceMade -= OnChoiceMade;
        }

        private void OnStart()
        {
            currentChoiceIndex = 0;
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);

            spawnCoroutine = StartCoroutine(SpawnNextChoice(1f));
        }

        private void OnChoiceMade()
        {
            if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
            currentChoiceIndex++;

            spawnCoroutine = StartCoroutine(SpawnNextChoice(delayBetweenChoices));
        }

        private IEnumerator SpawnNextChoice(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (GameManager.Instance == null || !GameManager.Instance.GameplayStarted)
                yield break;

            if (currentChoiceIndex >= playerFinanceManager.choiceDatas.ChoiceEntries.Length)
            {
                // Reset to loop choices, or end the game here.
                //BeatSaberVREvents.OnTriggerGameOver?.Invoke();
                //yield break;
                currentChoiceIndex = 0;
            }

            SpawnBlock(0, 0, BlockColor.Blue, (CutDirection)Random.Range(0, 4));
            SpawnBlock(1, 0, BlockColor.Red, (CutDirection)Random.Range(0, 4));
        }

        public void SpawnBlock(int col, int row, BlockColor color, CutDirection dir)
        {
            if (PoolManager.Instance == null) return;

            BlockBehavior block = PoolManager.Instance.GetBlock(color);
            if (block == null) return;

            block.transform.position = LaneHelper.GridToWorld(col, row, spawnZ);
            block.transform.rotation = Quaternion.identity;
            block.blockColor = color;
            block.cutDirection = dir;

            var choice = playerFinanceManager.choiceDatas.ChoiceEntries[currentChoiceIndex];
            block.Data = choice;

            block.choiceText.text = (color == BlockColor.Blue) ? choice.greenText : choice.redText;

            Sprite icon = (color == BlockColor.Blue) ? choice.greenChoiceIcon : choice.redChoiceIcon;
            if (icon != null) block.choiceSprite.sprite = icon;

            block.gameObject.SetActive(true);
            block.SetDirectionPoint(dir);
        }

        //public void SpawnWall(int col, int width, float zDepth)
        //{
        //    WallBehavior wall = PoolManager.Instance.GetWall();
        //    if (wall == null) return;

        //    float wallWidth = width * 0.7f;
        //    float leftEdge = -0.9f + col * 0.2f;
        //    float centerX = leftEdge + wallWidth * 0.5f;
        //    Vector3 pos = new Vector3(centerX, 1.8f, zDepth);

        //    wall.Setup(wallWidth, pos);
        //    wall.gameObject.SetActive(true);

        //    BeatSaberVREvents.OnBlockSpawned?.Invoke(BlockColor.Red); // Using Red as a placeholder since walls don't have a color.
        //}
    }
}