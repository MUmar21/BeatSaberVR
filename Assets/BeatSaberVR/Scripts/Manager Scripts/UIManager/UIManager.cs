using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace BeatSaberVR
{
    public class UIManager : Singleton<UIManager>
    {
        [Header("Panels")]
        [SerializeField] private GameObject startPanel;
        [SerializeField] private GameObject gameOverPanel;
        //[SerializeField] private GameObject swordSelectionPanel;
        [Header("Texts")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text timerText;
        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button replayButton;
        [Header("Game Over Panel")]
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text gameOverTitleText;
        [Header("User Rating")]
        [SerializeField] private GameObject userRatingCanvas;
        [SerializeField] private Image happinessFill;
        [SerializeField] private Image stressFill;
        [SerializeField] private Image financialFill;

        [Header("Sword Selection")]
        [SerializeField] private SwordSelection swordSelection;
        private const string SelectedSwordKey = "SelectedSword";
        string savedSword;

        public override void Awake()
        {
            base.Awake();
            //LoadSelectedSword();
            //UpdateSwordVisuals();
        }

        private void Start()
        {
            if (GameManager.Instance.PlayGameOnStart)
            {
                StartGameplay();
                return;
            }

            ToggleInGameUI(false);
            startPanel.SetActive(true);
            gameOverPanel.SetActive(false);
            //swordSelectionPanel.SetActive(true);
        }

        private void OnEnable()
        {
            startButton.onClick.AddListener(StartGameplay);
            replayButton.onClick.AddListener(OnReplay);
            //swordSelection.navLeftButton.onClick.AddListener(NavigateLeft);
            //swordSelection.navRightButton.onClick.AddListener(NavigateRight);
            //swordSelection.selectButton.onClick.AddListener(SelectSword);

            BeatSaberVREvents.OnGameplayEnd += OnEnd;
            BeatSaberVREvents.OnGameOver += OnGameOverScreen;
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveListener(StartGameplay);
            replayButton.onClick.RemoveListener(OnReplay);
            //swordSelection.navLeftButton.onClick.RemoveListener(NavigateLeft);
            //swordSelection.navRightButton.onClick.RemoveListener(NavigateRight);
            //swordSelection.selectButton.onClick.RemoveListener(SelectSword);

            BeatSaberVREvents.OnGameplayEnd -= OnEnd;
            BeatSaberVREvents.OnGameOver -= OnGameOverScreen;
        }

        private void StartGameplay()
        {
            startPanel.SetActive(false);
            //swordSelectionPanel.SetActive(false);
            ToggleInGameUI(true);
            BeatSaberVREvents.OnGameStart?.Invoke();
        }

        private void OnEnd()
        {
            ToggleInGameUI(false);
            gameOverTitleText.text = "COMPLETE!";
            finalScoreText.text = $"Score: {GameManager.Instance.GetScore()}";
            gameOverPanel.SetActive(true);
        }

        private void OnReplay()
        {
            gameOverPanel.SetActive(false);
            StartGameplay();
        }

        public void UpdateScoreAndComboTexts(int score)
        {
            scoreText.text = $"Score: {score}";
        }

        public void UpdateTimerText(float timeRemaining)
        {
            timerText.text = $"Time: {timeRemaining:F1}s";
        }

        public void UpdateFinanceRatesFill(float happy, float stress, float finance)
        {
            happinessFill.fillAmount = happy;
            stressFill.fillAmount = stress;
            financialFill.fillAmount = finance;
        }

        private void OnGameOverScreen(int finalScore)
        {
            gameOverTitleText.text = "FAILED";
            finalScoreText.text = $"Score: {finalScore}";
            gameOverPanel.SetActive(true);
        }

        private void ToggleInGameUI(bool toggle)
        {
            scoreText.gameObject.SetActive(toggle);
            timerText.gameObject.SetActive(toggle);
            userRatingCanvas.SetActive(toggle);
        }

        #region Sword Selection
        ////-----Sword Selection-----
        //private void SelectSword()
        //{
        //    Swords selectedSword = (Swords)swordSelection.currentIndex;
        //    BeatSaberVREvents.OnSwordSelected?.Invoke(selectedSword);
        //    swordSelection.selectedText.gameObject.SetActive(true);
        //    PlayerPrefs.SetString(SelectedSwordKey, selectedSword.ToString());
        //    savedSword = selectedSword.ToString();
        //}

        //private void NavigateLeft()
        //{
        //    swordSelection.currentIndex--;
        //    if (swordSelection.currentIndex < 0)
        //        swordSelection.currentIndex = swordSelection.swordDatas.Length - 1;

        //    UpdateSwordVisuals();
        //}

        //private void NavigateRight()
        //{
        //    swordSelection.currentIndex++;
        //    if (swordSelection.currentIndex >= swordSelection.swordDatas.Length)
        //        swordSelection.currentIndex = 0;

        //    UpdateSwordVisuals();
        //}

        //private void UpdateSwordVisuals()
        //{
        //    if (swordSelection.swordDatas == null || swordSelection.swordDatas.Length == 0) return;

        //    for (int i = 0; i < swordSelection.swordDatas.Length; i++)
        //    {
        //        if (swordSelection.swordDatas[i].gameObject != null)
        //        {
        //            swordSelection.swordDatas[i].gameObject.SetActive(i == swordSelection.currentIndex);
        //        }
        //    }
        //    swordSelection.selectedText.gameObject.SetActive(PlayerPrefs.GetString(SelectedSwordKey) == swordSelection.swordDatas[swordSelection.currentIndex].sword.ToString());
        //}

        //private void LoadSelectedSword()
        //{
        //    savedSword = PlayerPrefs.GetString(SelectedSwordKey, Swords.SwordA.ToString());
        //    try
        //    {
        //        Swords savedEnum = (Swords)System.Enum.Parse(typeof(Swords), savedSword);
        //        swordSelection.currentIndex = (int)savedEnum;
        //    }
        //    catch
        //    {
        //        swordSelection.currentIndex = 0;
        //    }

        //    Swords selectedSword = (Swords)swordSelection.currentIndex;
        //    BeatSaberVREvents.OnSwordSelected?.Invoke(selectedSword);
        //}
        #endregion
    }

    [System.Serializable]
    public class SwordSelection
    {
        public SwordData[] swordDatas;
        public Button navLeftButton;
        public Button navRightButton;
        public Button selectButton;
        public TMP_Text selectedText;
        public int currentIndex;
    }
}