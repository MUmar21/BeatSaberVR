using UnityEngine;

namespace BeatSaberVR
{
    using DG.Tweening;

    public class EnvironmentRotator : MonoBehaviour
    {
        [Header("Rotation Settings")]
        public Vector3 rotationAxis = Vector3.forward;
        public float rotationAmount = 60f;
        public Ease easeType = Ease.InOutQuad;

        [Header("References")]
        [SerializeField] private BeatMapSO beatMap;
        [SerializeField] private AudioSource songSource;

        [Header("Randomization")]
        public bool randomizeStartRotation = true;

        private float currentBeatDuration = 1f;
        private bool isPlaying = false;

        private void OnEnable()
        {
            BeatSaberVREvents.OnGameStart += OnStart;
            BeatSaberVREvents.OnGameplayEnd += OnEnd;
        }

        private void OnDisable()
        {
            BeatSaberVREvents.OnGameStart -= OnStart;
            BeatSaberVREvents.OnGameplayEnd -= OnEnd;
        }

        private void OnStart()
        {
            if (randomizeStartRotation)
            {
                transform.localRotation = Quaternion.Euler(rotationAxis * Random.Range(0, 360f));
            }

            CalculateDurationFromBPM();
            PlayRandomRotation();
        }

        private void Update()
        {
            if (!GameManager.Instance.GameplayStarted) return;

            // Sync with the song playback state
            if (songSource != null && songSource.isPlaying && !isPlaying)
            {
                isPlaying = true;
                UpdateRotationSpeed(currentBeatDuration * 0.5f, currentBeatDuration);
            }
            else if (songSource != null && !songSource.isPlaying && isPlaying)
            {
                isPlaying = false;
            }
        }

        private void CalculateDurationFromBPM()
        {
            if (beatMap != null && beatMap.bpm > 0)
            {
                // One beat in seconds = 60 / BPM
                currentBeatDuration = 60f / beatMap.bpm;
            }
            else
            {
                currentBeatDuration = 1f; // Fallback
            }
        }

        private void PlayRandomRotation()
        {
            transform.DOKill();

            float direction = Random.value > 0.5f ? 1f : -1f;

            float duration = Random.Range(currentBeatDuration * 0.8f, currentBeatDuration * 1.2f);

            transform.DOLocalRotate(rotationAxis * (rotationAmount * direction), duration, RotateMode.LocalAxisAdd)
                .SetEase(easeType)
                .OnComplete(() =>
                {
                    if (this.gameObject.activeInHierarchy)
                        PlayRandomRotation();
                });
        }

        public void UpdateRotationSpeed(float minMultiplier, float maxMultiplier)
        {
            float newMin = currentBeatDuration * minMultiplier;
            float newMax = currentBeatDuration * maxMultiplier;

            PlayRandomRotation();
        }

        private void OnEnd()
        {
            transform.DOKill();
            transform.localRotation = Quaternion.identity;
            isPlaying = false;
        }
    }
}