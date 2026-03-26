using System.Collections;
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
        [SerializeField] private CanvasGroup wallHitEffect;
        [Header("Texts")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text comboText;
        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button replayButton;
        [Header("Energy Bar")]
        [SerializeField] private Image energyBarFill;
        [Header("Game Over Panel")]
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text gameOverTitleText;

        private Coroutine wallHitCoroutine;

        private void Start()
        {
            startPanel.SetActive(true);
            gameOverPanel.SetActive(false);
            if (wallHitEffect != null) wallHitEffect.alpha = 0f;
        }

        private void OnEnable()
        {
            startButton.onClick.AddListener(OnStart);
            replayButton.onClick.AddListener(OnReplay);

            BeatSaberVREvents.OnGameplayEnd += OnEnd;
            BeatSaberVREvents.OnGameOver += OnGameOverScreen;
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveListener(OnStart);
            replayButton.onClick.RemoveListener(OnReplay);

            BeatSaberVREvents.OnGameplayEnd -= OnEnd;
            BeatSaberVREvents.OnGameOver -= OnGameOverScreen;
        }

        private void OnStart()
        {
            if (wallHitEffect != null) wallHitEffect.alpha = 0f;
            startPanel.SetActive(false);
            BeatSaberVREvents.OnGameStart?.Invoke();
        }

        private void OnEnd()
        {
            if (wallHitEffect != null) wallHitEffect.alpha = 0f;
            gameOverTitleText.text = "COMPLETE!";
            finalScoreText.text = $"Score: {GameManager.Instance.GetScore()}";
            gameOverPanel.SetActive(true);
        }

        private void OnReplay()
        {
            if (wallHitEffect != null) wallHitEffect.alpha = 0f;
            startPanel.SetActive(false);
            gameOverPanel.SetActive(false);
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

            if (wallHitCoroutine != null)
            {
                StopCoroutine(wallHitCoroutine);
                wallHitCoroutine = null;
            }

            wallHitEffect.alpha = 1f;
            wallHitCoroutine = StartCoroutine(PlayWallHitEffect());
        }

        private IEnumerator PlayWallHitEffect()
        {
            if (wallHitEffect == null)
            {
                yield break;
            }

            float duration = 1f;
            float elapsed = 0f;
            float startAlpha = wallHitEffect.alpha;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                wallHitEffect.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }

            wallHitEffect.alpha = 0f;
            wallHitCoroutine = null;
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
    }
}