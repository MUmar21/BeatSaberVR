using DG.Tweening;
using UnityEngine;

namespace BeatSaberVR
{
    public class FogSync : MonoBehaviour
    {
        [Header("Sync Settings")]
        [SerializeField] private ColorTransitioner masterTransitioner;

        [Range(0f, 1f)]
        public float fogBrightnessMultiplier = 0.5f;
        private const string FogTweenId = "FogTween";

        private void OnEnable()
        {
            if (masterTransitioner != null)
            {
                masterTransitioner.OnColorChanged += SyncFogColor;
                masterTransitioner.OnFlashTriggered += SyncFogFlash;
            }
        }

        private void OnDisable()
        {
            if (masterTransitioner != null)
            {
                masterTransitioner.OnColorChanged -= SyncFogColor;
                masterTransitioner.OnFlashTriggered -= SyncFogFlash;
            }
            DOTween.Kill(FogTweenId);
        }

        private void SyncFogColor(Color targetColor, float duration)
        {
            DOTween.Kill(FogTweenId);

            Color finalColor = targetColor * fogBrightnessMultiplier;

            DOVirtual.Color(RenderSettings.fogColor, finalColor, duration, (c) =>
            {
                RenderSettings.fogColor = c;
            }).SetId(FogTweenId);
        }

        private void SyncFogFlash(Color flashColor, float duration)
        {
            DOTween.Kill(FogTweenId);
            RenderSettings.fogColor = flashColor * fogBrightnessMultiplier;
        }
    }
}