using UnityEngine;

namespace BeatSaberVR
{
    using DG.Tweening;

    public class WallHitEffectHandler : MonoBehaviour
    {
        [Header("Slow Motion Settings")]
        [SerializeField] private float slowMoScale = 0.4f; // Lowered to make it more obvious
        [SerializeField] private float slowMoDuration = 0.4f;

        [Header("VR Safe Shake Settings")]
        [SerializeField] private Transform cameraRigTarget; // MUST BE A PARENT OF THE CAMERA
        [SerializeField] private float shakeStrength = 0.15f; // Increased for visibility
        [SerializeField] private int shakeVibrato = 30;
        [SerializeField] private float shakeDuration = 0.2f;

        private Vector3 originalRigPosition;
        private const string WallTimeTweenId = "WallTimeTween";

        private void Awake()
        {
            if (cameraRigTarget != null)
                originalRigPosition = cameraRigTarget.localPosition;
            else
                Debug.LogWarning("WallHitEffectHandler: cameraRigTarget is missing! Shake won't work.");
        }

        private void OnEnable() => BeatSaberVREvents.OnWallHit += TriggerWallHitEffect;
        private void OnDisable()
        {
            BeatSaberVREvents.OnWallHit -= TriggerWallHitEffect;
            ResetTimeScale();
        }

        private void TriggerWallHitEffect()
        {
            // Debug the actual scale to see if it's changing in the console
            Debug.Log($"Wall hit! Target TimeScale: {slowMoScale}");

            PlaySlowMo();
            PlayVRSafeShake();
        }

        private void PlaySlowMo()
        {
            DOTween.Kill(WallTimeTweenId);

            Sequence seq = DOTween.Sequence().SetId(WallTimeTweenId).SetUpdate(true);

            // Dip
            seq.Append(DOTween.To(() => Time.timeScale, x => SetTimeScale(x), slowMoScale, 0.05f)
                .SetEase(Ease.OutQuad));

            seq.AppendInterval(0.08f);

            // Recovery
            seq.Append(DOTween.To(() => Time.timeScale, x => SetTimeScale(x), 1f, slowMoDuration)
                .SetEase(Ease.OutCubic));
        }

        private void PlayVRSafeShake()
        {
            if (cameraRigTarget == null) return;

            cameraRigTarget.DOKill();
            cameraRigTarget.localPosition = originalRigPosition;

            // Using DOShakePosition on a parent container
            cameraRigTarget.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato)
                .SetUpdate(true)
                .OnComplete(() => cameraRigTarget.localPosition = originalRigPosition);
        }

        private void SetTimeScale(float scale)
        {
            Time.timeScale = scale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }

        private void ResetTimeScale()
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }
}