using DG.Tweening;
using UnityEngine;

namespace BeatSaberVR
{
    public class BlockAppearAnimation : MonoBehaviour
    {
        [Header("Appearance Timing")]
        public float duration = 0.4f;
        public float overshoot = 1.2f;

        [Header("Animation Settings")]
        public float groundY = 0.4f; // The exact height of your floor

        private float targetY;
        private Sequence appearSequence;

        void OnEnable()
        {
            PlayAppearAnimation();
        }

        public void PlayAppearAnimation()
        {
            appearSequence?.Kill();

            // 1. Capture the target height assigned by the BlockSpawner (e.g., Row 0, 1, or 2)
            targetY = transform.localPosition.y;

            // 2. Set the initial state for the spawn illusion
            transform.localScale = Vector3.zero;

            // Force the block down to the ground to start
            Vector3 startPos = transform.localPosition;
            startPos.y = groundY;
            transform.localPosition = startPos;

            // Give it a wild starting rotation so it "tumbles" into place
            transform.localRotation = Quaternion.Euler(45f, 0f, 25f);

            // 3. Create the animation sequence
            appearSequence = DOTween.Sequence();

            // Scale up with a bouncy overshoot
            appearSequence.Join(transform.DOScale(1f, duration)
                .SetEase((DG.Tweening.Ease)Ease.EaseInOutBack, overshoot));

            // Rotate back to zero perfectly
            appearSequence.Join(transform.DOLocalRotate(Vector3.zero, duration)
                .SetEase((DG.Tweening.Ease)Ease.EaseInOutBack));

            // Only animate the Y-Axis! (Let BlockBehavior control the Z-Axis)
            // If the target is higher than the ground, it will jump up. 
            // If the target is the bottom row (0.4f), it will just stay on the ground.
            if (targetY > groundY + 0.05f)
            {
                appearSequence.Join(transform.DOLocalMoveY(targetY, duration)
                    .SetEase((DG.Tweening.Ease)Ease.EaseOutCubic));
            }
        }

        void OnDisable()
        {
            appearSequence?.Kill();
        }
    }
}