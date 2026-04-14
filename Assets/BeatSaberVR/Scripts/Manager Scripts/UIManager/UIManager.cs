using DG.Tweening;
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
        [SerializeField] private float fillDuration = 0.5f;
        [Header("Line Visuals")]
        [SerializeField] private GameObject leftLineVisual;
        [SerializeField] private GameObject rightLineVisual;

        public override void Awake()
        {
            base.Awake();
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
        }

        private void OnEnable()
        {
            startButton.onClick.AddListener(StartGameplay);
            replayButton.onClick.AddListener(OnReplay);

            BeatSaberVREvents.OnGameplayEnd += OnEnd;
            BeatSaberVREvents.OnGameOver += OnGameOverScreen;
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveListener(StartGameplay);
            replayButton.onClick.RemoveListener(OnReplay);

            BeatSaberVREvents.OnGameplayEnd -= OnEnd;
            BeatSaberVREvents.OnGameOver -= OnGameOverScreen;
        }

        private void StartGameplay()
        {
            startPanel.SetActive(false);
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
            if (timerText == null) return;

            timeRemaining = Mathf.Max(0f, timeRemaining);
            int minutes = (int)(timeRemaining / 60f);
            int seconds = (int)(timeRemaining % 60f);
            timerText.text = $"Time: {minutes}:{seconds:00}";
        }

        public void UpdateFinanceRatesFill(float happy, float stress, float finance)
        {
            happinessFill?.DOKill();
            stressFill?.DOKill();
            financialFill?.DOKill();

            happinessFill.DOFillAmount(happy, fillDuration).SetEase(Ease.OutQuad);
            stressFill.DOFillAmount(stress, fillDuration).SetEase(Ease.OutQuad);
            financialFill.DOFillAmount(finance, fillDuration).SetEase(Ease.OutQuad);
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
            leftLineVisual.SetActive(!toggle);
            rightLineVisual.SetActive(!toggle);
        }
    }
}