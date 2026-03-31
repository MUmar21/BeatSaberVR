using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace BeatSaberVR
{
    using DG.Tweening;

    public class UIManager : Singleton<UIManager>
    {
        [Header("Panels")]
        [SerializeField] private GameObject startPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject swordSelectionPanel;
        [SerializeField] private CanvasGroup wallHitEffect;
        [Header("Texts")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text comboText;
        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button replayButton;
        [Header("Energy Bar")]
        [SerializeField] private GameObject fillCanvas;
        [SerializeField] private Image energyBarFill;
        [Header("Game Over Panel")]
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text gameOverTitleText;

        [Header("Sword Selection")]
        [SerializeField] private SwordSelection swordSelection;
        private const string SelectedSwordKey = "SelectedSword";
        string savedSword;

        public override void Awake()
        {
            base.Awake();
            LoadSelectedSword();
            UpdateSwordVisuals();
        }

        private void Start()
        {
            ToggleInGameUI(false);
            startPanel.SetActive(true);
            swordSelectionPanel.SetActive(true);
            gameOverPanel.SetActive(false);
            if (wallHitEffect != null) wallHitEffect.alpha = 0f;
        }

        private void OnEnable()
        {
            startButton.onClick.AddListener(OnStart);
            replayButton.onClick.AddListener(OnReplay);
            swordSelection.navLeftButton.onClick.AddListener(NavigateLeft);
            swordSelection.navRightButton.onClick.AddListener(NavigateRight);
            swordSelection.selectButton.onClick.AddListener(SelectSword);

            BeatSaberVREvents.OnGameplayEnd += OnEnd;
            BeatSaberVREvents.OnGameOver += OnGameOverScreen;
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveListener(OnStart);
            replayButton.onClick.RemoveListener(OnReplay);
            swordSelection.navLeftButton.onClick.RemoveListener(NavigateLeft);
            swordSelection.navRightButton.onClick.RemoveListener(NavigateRight);
            swordSelection.selectButton.onClick.RemoveListener(SelectSword);

            BeatSaberVREvents.OnGameplayEnd -= OnEnd;
            BeatSaberVREvents.OnGameOver -= OnGameOverScreen;
        }

        private void OnStart()
        {
            if (wallHitEffect != null) wallHitEffect.alpha = 0f;
            startPanel.SetActive(false);
            swordSelectionPanel.SetActive(false);
            ToggleInGameUI(true);
            BeatSaberVREvents.OnGameStart?.Invoke();
        }

        private void OnEnd()
        {
            if (wallHitEffect != null) wallHitEffect.alpha = 0f;
            gameOverTitleText.text = "COMPLETE!";
            finalScoreText.text = $"Score: {GameManager.Instance.GetScore()}";
            ToggleInGameUI(false);
            gameOverPanel.SetActive(true);
        }

        private void OnReplay()
        {
            if (wallHitEffect != null) wallHitEffect.alpha = 0f;
            startPanel.SetActive(false);
            swordSelectionPanel.SetActive(false);
            gameOverPanel.SetActive(false);
            ToggleInGameUI(true);
            BeatSaberVREvents.OnGameStart?.Invoke();
        }

        public void UpdateScoreAndComboTexts(int score, int combo)
        {
            scoreText.text = $"Score: {score}";
            comboText.text = $"Combo: {combo}";
        }

        public void OnWallHit()
        {
            if (wallHitEffect == null) return;

            wallHitEffect.alpha = 1f;
            PlayWallHitEffect();
        }

        private void PlayWallHitEffect()
        {
            if (wallHitEffect == null) return;
            DOTween.Kill(wallHitEffect);
            wallHitEffect.alpha = 1f;
            wallHitEffect.DOFade(0f, 1.5f)
                .SetEase(Ease.InCubic)
                .SetId(wallHitEffect);
        }

        public void UpdateEnergyBar(float normalizedValue)
        {
            if (energyBarFill == null) return;
            energyBarFill.fillAmount = normalizedValue;
            energyBarFill.color = Color.Lerp(Color.red, Color.green, normalizedValue);
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
            comboText.gameObject.SetActive(toggle);
            fillCanvas.gameObject.SetActive(toggle);
        }

        //-----Sword Selection-----
        private void SelectSword()
        {
            Swords selectedSword = (Swords)swordSelection.currentIndex;
            BeatSaberVREvents.OnSwordSelected?.Invoke(selectedSword);
            swordSelection.selectedText.gameObject.SetActive(true);
            PlayerPrefs.SetString(SelectedSwordKey, selectedSword.ToString());
            savedSword = selectedSword.ToString();
        }

        private void NavigateLeft()
        {
            swordSelection.currentIndex--;
            if (swordSelection.currentIndex < 0)
                swordSelection.currentIndex = swordSelection.swordDatas.Length - 1;

            UpdateSwordVisuals();
        }

        private void NavigateRight()
        {
            swordSelection.currentIndex++;
            if (swordSelection.currentIndex >= swordSelection.swordDatas.Length)
                swordSelection.currentIndex = 0;

            UpdateSwordVisuals();
        }

        private void UpdateSwordVisuals()
        {
            if (swordSelection.swordDatas == null || swordSelection.swordDatas.Length == 0) return;

            for (int i = 0; i < swordSelection.swordDatas.Length; i++)
            {
                if (swordSelection.swordDatas[i].gameObject != null)
                {
                    swordSelection.swordDatas[i].gameObject.SetActive(i == swordSelection.currentIndex);
                }
            }
            swordSelection.selectedText.gameObject.SetActive(PlayerPrefs.GetString(SelectedSwordKey) == swordSelection.swordDatas[swordSelection.currentIndex].sword.ToString());
        }

        private void LoadSelectedSword()
        {
            savedSword = PlayerPrefs.GetString(SelectedSwordKey, Swords.SwordA.ToString());
            try
            {
                Swords savedEnum = (Swords)System.Enum.Parse(typeof(Swords), savedSword);
                swordSelection.currentIndex = (int)savedEnum;
            }
            catch
            {
                swordSelection.currentIndex = 0;
            }

            Swords selectedSword = (Swords)swordSelection.currentIndex;
            BeatSaberVREvents.OnSwordSelected?.Invoke(selectedSword);
        }
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