using DG.Tweening;
using UnityEngine;

namespace BeatSaberVR
{
    public class BlockAppearAnimation : MonoBehaviour
    {
        [Header("Appearance Timing")]
        public float duration = 0.4f;
        public float overshoot = 1.2f;

        [Header("Force Shoot Settings")]
        public float spawnDistance = 15f;
        public float spawnVerticalOffset = -1.5f;

        private Vector3 targetLocalPos;
        private Sequence appearSequence;
        private BlockBehavior blockBehavior;

        void Awake()
        {
            blockBehavior = GetComponent<BlockBehavior>();
        }

        void OnEnable()
        {
            PlayAppearAnimation();
        }

        public void PlayAppearAnimation()
        {
            appearSequence?.Kill();

            targetLocalPos = transform.localPosition;

            transform.localScale = Vector3.zero;
            transform.localPosition = targetLocalPos + (Vector3.forward * spawnDistance) + (Vector3.up * spawnVerticalOffset);
            if (blockBehavior != null) blockBehavior.isPaused = true;

            appearSequence = DOTween.Sequence();

            appearSequence.Join(transform.DOLocalMove(targetLocalPos, duration).SetEase((DG.Tweening.Ease)Ease.EaseOutBack));

            appearSequence.Join(transform.DOScale(1f, duration).SetEase((DG.Tweening.Ease)Ease.EaseOutBack, overshoot));

            transform.localRotation = Quaternion.Euler(30, 0, 15);
            appearSequence.Join(transform.DOLocalRotate(Vector3.zero, duration).SetEase((DG.Tweening.Ease)Ease.EaseOutBack));

            appearSequence.OnComplete(() =>
            {
                if (blockBehavior != null) blockBehavior.isPaused = false;
            });
        }

        void OnDisable()
        {
            appearSequence?.Kill();
        }
    }
}