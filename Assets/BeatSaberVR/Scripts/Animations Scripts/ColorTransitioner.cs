using UnityEngine;

namespace BeatSaberVR
{
    using DG.Tweening;

    public class ColorTransitioner : MonoBehaviour
    {
        [Header("Settings")]
        public Renderer targetRenderer;
        [ColorUsage(true, true)] public Color[] possibleColors;
        public float colorStayDuration = 1.0f;
        public float snapDuration = 0.05f;

        [Header("Block Cut Flash")]
        public float flashIntensity = 4.0f;
        public float flashDuration = 0.2f;

        private Material targetMaterial;
        private int lastColorIndex = -1;
        private Sequence currentSequence;

        private static readonly int ColorProp = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionProp = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
            targetMaterial = targetRenderer.material;
        }

        private void OnEnable()
        {
            BeatSaberVREvents.OnGameStart += StartNextTransition;
            BeatSaberVREvents.OnBlockSpawned += TriggerFlash;
            BeatSaberVREvents.OnBlockCut += TriggerFlash;
            BeatSaberVREvents.OnGameplayEnd += OnEnd;
        }

        private void OnDisable()
        {
            BeatSaberVREvents.OnGameStart -= StartNextTransition;
            BeatSaberVREvents.OnBlockSpawned -= TriggerFlash;
            BeatSaberVREvents.OnBlockCut -= TriggerFlash;
            BeatSaberVREvents.OnGameplayEnd -= OnEnd;
        }

        private void StartNextTransition()
        {
            currentSequence?.Kill();

            int nextIndex;
            do
            {
                nextIndex = Random.Range(0, possibleColors.Length);
            } while (nextIndex == lastColorIndex && possibleColors.Length > 1);

            lastColorIndex = nextIndex;
            Color targetColor = possibleColors[nextIndex];

            currentSequence = DOTween.Sequence();

            currentSequence.Append(targetMaterial.DOColor(targetColor, ColorProp, snapDuration));
            currentSequence.Join(targetMaterial.DOColor(targetColor, EmissionProp, snapDuration));

            currentSequence.AppendInterval(colorStayDuration);

            currentSequence.OnComplete(StartNextTransition);
        }

        private void TriggerFlash(BlockColor color)
        {
            currentSequence?.Kill();
            DOTween.Kill(targetMaterial);

            Color baseColor = color == BlockColor.Red ? Color.red : Color.blue;
            Color intensityColor = baseColor * flashIntensity;

            targetMaterial.SetColor(ColorProp, baseColor);
            targetMaterial.SetColor(EmissionProp, intensityColor);

            targetMaterial.DOColor(baseColor, EmissionProp, flashDuration)
                .OnComplete(StartNextTransition);
        }

        private void OnEnd()
        {
            currentSequence?.Kill();
            DOTween.Kill(targetMaterial);
        }
    }
}