using DG.Tweening;
using UnityEngine;

namespace BeatSaberVR
{
    public class ObjectEmphasisTween : MonoBehaviour
    {
        [Header("Oscillation Settings")]
        [SerializeField] private bool enableOscillation = true;
        [SerializeField] private float moveDistance = 0.05f;
        [SerializeField] private float moveDuration = 1f;

        [Header("Pulse Settings")]
        [SerializeField] private bool enablePulse = true;
        [SerializeField] private float scaleTarget = 1.15f;
        [SerializeField] private float scaleDuration = 0.6f;

        private Vector3 _initialLocalPos;
        private Vector3 _initialScale;
        private Sequence _emphasisSequence;

        private void Awake()
        {
            _initialLocalPos = transform.localPosition;
            _initialScale = transform.localScale;
        }

        private void OnEnable()
        {
            PlayEmphasis();
        }

        public void PlayEmphasis()
        {
            _emphasisSequence?.Kill();
            _emphasisSequence = DOTween.Sequence();

            if (enableOscillation)
            {
                _emphasisSequence.Join(transform.DOLocalMoveY(_initialLocalPos.y + moveDistance, moveDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo));
            }

            if (enablePulse)
            {
                _emphasisSequence.Join(transform.DOScale(_initialScale * scaleTarget, scaleDuration)
                    .SetEase(Ease.InOutQuad)
                    .SetLoops(-1, LoopType.Yoyo));
            }

            _emphasisSequence.SetLink(gameObject);
        }

        private void OnDisable()
        {
            _emphasisSequence?.Kill();

            transform.localPosition = _initialLocalPos;
            transform.localScale = _initialScale;
        }
    }
}