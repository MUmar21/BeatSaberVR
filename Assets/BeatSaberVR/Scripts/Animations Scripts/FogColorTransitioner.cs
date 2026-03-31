using DG.Tweening;
using UnityEngine;

namespace BeatSaberVR
{
    public class FogColorTransitioner : MonoBehaviour
    {
        [Header("Fog Settings")]
        [ColorUsage(false, false)] public Color[] possibleFogColors;
        public float colorStayDuration = 2.0f;
        public float snapDuration = 0.1f;

        [Header("Flash Settings")]
        public float flashDuration = 0.25f;

        private int lastColorIndex = -1;
        private Sequence currentSequence;

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

            currentSequence?.Kill();
        }

        private void StartNextTransition()
        {
            currentSequence?.Kill();

            int nextIndex;
            do
            {
                nextIndex = Random.Range(0, possibleFogColors.Length);
            } while (nextIndex == lastColorIndex && possibleFogColors.Length > 1);

            lastColorIndex = nextIndex;
            Color targetColor = possibleFogColors[nextIndex];

            currentSequence = DOTween.Sequence();

            currentSequence.Append(DOVirtual.Color(RenderSettings.fogColor, targetColor, snapDuration, (c) =>
            {
                RenderSettings.fogColor = c;
            }));

            currentSequence.AppendInterval(colorStayDuration);
            currentSequence.OnComplete(StartNextTransition);
        }

        private void TriggerFlash(BlockColor color)
        {
            currentSequence?.Kill();

            Color flashColor = color == BlockColor.Red ? Color.red : Color.blue;

            // Instant snap to flash color
            RenderSettings.fogColor = flashColor;

            // Fade back to a random environment color
            DOVirtual.Color(flashColor, GetRandomPossibleColor(), flashDuration, (c) =>
            {
                RenderSettings.fogColor = c;
            }).OnComplete(StartNextTransition);
        }

        private Color GetRandomPossibleColor()
        {
            if (possibleFogColors == null || possibleFogColors.Length == 0) return Color.black;
            return possibleFogColors[Random.Range(0, possibleFogColors.Length)];
        }

        private void OnEnd()
        {
            currentSequence?.Kill();
        }
    }
}