using UnityEngine;

namespace BeatSaberVR
{
    using DG.Tweening;

    public class EnvironmentLightingSync : MonoBehaviour
    {
        [Header("Master Sync")]
        [SerializeField] private ColorTransitioner masterTransitioner;

        [Header("Targets")]
        [Tooltip("Assign your main Directional Light here.")]
        [SerializeField] private Light directionalLight;

        [Header("Brightness Multipliers")]
        [Range(0f, 2f)] public float fogMultiplier = 0.5f;
        [Range(0f, 2f)] public float ambientMultiplier = 0.3f;
        [Range(0f, 2f)] public float lightMultiplier = 1.0f;

        private const string SyncTweenId = "EnvSyncTween";

        private void OnEnable()
        {
            if (masterTransitioner != null)
            {
                masterTransitioner.OnColorChanged += SyncAllColors;
                masterTransitioner.OnFlashTriggered += SyncAllFlashes;
            }
        }

        private void OnDisable()
        {
            if (masterTransitioner != null)
            {
                masterTransitioner.OnColorChanged -= SyncAllColors;
                masterTransitioner.OnFlashTriggered -= SyncAllFlashes;
            }
            DOTween.Kill(SyncTweenId);
        }

        private void SyncAllColors(Color targetColor, float duration)
        {
            DOTween.Kill(SyncTweenId);

            Color targetFog = targetColor * fogMultiplier;
            Color targetAmbient = targetColor * ambientMultiplier;
            Color targetLight = targetColor * lightMultiplier;

            Color startFog = RenderSettings.fogColor;
            Color startAmbient = RenderSettings.ambientLight;
            Color startLight = directionalLight != null ? directionalLight.color : Color.black;

            // Run ONE single float tween from 0 to 1 to drive all three transitions simultaneously
            DOVirtual.Float(0f, 1f, duration, (t) =>
            {
                RenderSettings.fogColor = Color.Lerp(startFog, targetFog, t);
                RenderSettings.ambientLight = Color.Lerp(startAmbient, targetAmbient, t);

                if (directionalLight != null)
                    directionalLight.color = Color.Lerp(startLight, targetLight, t);

            }).SetId(SyncTweenId).SetEase(Ease.OutQuad);
        }

        private void SyncAllFlashes(Color flashColor, float duration)
        {
            DOTween.Kill(SyncTweenId);

            RenderSettings.fogColor = flashColor * fogMultiplier;
            RenderSettings.ambientLight = flashColor * ambientMultiplier;

            if (directionalLight != null)
                directionalLight.color = flashColor * lightMultiplier;
        }
    }
}