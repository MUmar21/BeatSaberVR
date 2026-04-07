using UnityEngine;

namespace BeatSaberVR
{
    using DG.Tweening;

    public class BlockAppearAnimation : MonoBehaviour
    {
        [Header("Appearance Timing")]
        public float duration = 0.4f;
        public float overshoot = 1.2f;

        private float targetY;
        private Sequence appearSequence;

        private BoxCollider blockCollider;

        private void Awake()
        {
            blockCollider = GetComponent<BoxCollider>();
        }

        void OnEnable()
        {
            blockCollider.enabled = false;
            PlayAppearAnimation();
        }

        public void PlayAppearAnimation()
        {
            appearSequence?.Kill();

            targetY = transform.localPosition.y;
            transform.localScale = Vector3.zero;

            Vector3 startPos = transform.localPosition;
            transform.localPosition = startPos;

            transform.localRotation = Quaternion.Euler(45f, 0f, 25f);

            appearSequence = DOTween.Sequence();

            appearSequence.Join(transform.DOScale(Vector3.one, duration)
                .SetEase(Ease.InOutBack, overshoot));

            appearSequence.OnComplete(() =>
            {
                blockCollider.enabled = true;
            });
        }

        void OnDisable()
        {
            appearSequence?.Kill();
        }
    }
}