using System.Collections;
using UnityEngine;

namespace BeatSaberVR
{
    public class BlockAppearAnimation : MonoBehaviour
    {
        [Header("Animation Settings")]
        public float duration = 0.3f;
        public float overshoot = 1.15f;
        public float jumpHeight = 0.15f;

        private Coroutine currentAnim;

        void OnEnable()
        {
            if (currentAnim != null) StopCoroutine(currentAnim);
            transform.localScale = Vector3.zero;
            currentAnim = StartCoroutine(AnimateIn());
        }

        void OnDisable()
        {
            if (currentAnim != null)
            {
                StopCoroutine(currentAnim);
                currentAnim = null;
            }
            transform.localScale = Vector3.one;
        }

        IEnumerator AnimateIn()
        {
            Vector3 targetPos = transform.position;
            Vector3 startPos = targetPos + Vector3.up * jumpHeight;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                transform.localScale = Vector3.one * SpringCurve(t);

                // Settle from slightly above to target position
                transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            transform.localScale = Vector3.one;
            transform.position = targetPos;
            currentAnim = null;
        }

        private float SpringCurve(float t)
        {
            if (t < 0.6f) return Mathf.Lerp(0f, overshoot, t / 0.6f);
            return Mathf.Lerp(overshoot, 1f, (t - 0.6f) / 0.4f);
        }
    }
}