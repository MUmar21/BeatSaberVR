using DG.Tweening;
using UnityEngine;

namespace BeatSaberVR
{
    public class BlockAppearAnimation : MonoBehaviour
    {
        [Header("Appearance Settings")]
        public float appearDuration = 0.35f;
        public float popOvershoot = 1.5f;

        [Header("Disappear Settings")]
        public float hideDuration = 0.25f;

        private Sequence animationSequence;
        private BoxCollider blockCollider;

        private void Awake()
        {
            blockCollider = GetComponent<BoxCollider>();
        }

        private void OnEnable()
        {
            blockCollider.enabled = false;
            transform.localScale = Vector3.zero;

            PlayPopIn();
        }

        public void PlayPopIn()
        {
            animationSequence?.Kill();
            animationSequence = DOTween.Sequence();

            animationSequence.Join(transform.DOScale(Vector3.one, appearDuration)
                .SetEase(Ease.OutBack, popOvershoot));

            animationSequence.OnComplete(() =>
            {
                blockCollider.enabled = true;
            });
        }

        public void PlayPopOut(System.Action onComplete = null)
        {
            blockCollider.enabled = false; // Prevent hits during animation
            animationSequence?.Kill();
            animationSequence = DOTween.Sequence();

            animationSequence.Append(transform.DOScale(Vector3.one * 1.15f, hideDuration * 0.3f).SetEase(Ease.OutQuad));
            animationSequence.Append(transform.DOScale(Vector3.zero, hideDuration * 0.7f).SetEase(Ease.InBack));

            animationSequence.OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }

        private void OnDisable()
        {
            animationSequence?.Kill();
        }
    }
}