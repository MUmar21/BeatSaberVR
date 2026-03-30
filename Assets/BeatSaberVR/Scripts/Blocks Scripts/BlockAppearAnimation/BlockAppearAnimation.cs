using UnityEngine;

namespace BeatSaberVR
{
    using DG.Tweening;

    public class BlockAppearAnimation : MonoBehaviour
    {
        [Header("Appearance Timing")]
        public float duration = 0.4f;
        public float overshoot = 1.2f;
        public float rotationDelay = 0.2f;

        [Header("Animation Settings")]
        public float groundY = 0.4f;

        private float targetY;
        private Sequence appearSequence;

        void OnEnable()
        {
            PlayAppearAnimation();
        }

        public void PlayAppearAnimation()
        {
            appearSequence?.Kill();

            targetY = transform.localPosition.y;
            transform.localScale = Vector3.zero;

            Vector3 startPos = transform.localPosition;
            startPos.y = groundY;
            transform.localPosition = startPos;

            transform.localRotation = Quaternion.Euler(45f, 0f, 25f);

            appearSequence = DOTween.Sequence();

            appearSequence.Join(transform.DOScale(Vector3.one, duration)
                .SetEase(Ease.InOutBack, overshoot));

            appearSequence.Insert(rotationDelay, transform.DOLocalRotate(Vector3.zero, duration, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutBack));

            if (targetY > groundY + 0.05f)
            {
                appearSequence.Join(transform.DOLocalMoveY(targetY, duration)
                    .SetEase(Ease.OutCubic));
            }
        }

        void OnDisable()
        {
            appearSequence?.Kill();
        }
    }
}