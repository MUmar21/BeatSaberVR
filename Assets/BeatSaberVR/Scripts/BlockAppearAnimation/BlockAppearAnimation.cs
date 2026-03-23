using System.Collections;
using UnityEngine;
namespace BeatSaberVR
{
    public class BlockAppearAnimation : MonoBehaviour
    {

        [Header("Animation Settings")]
        public float duration = 0.3f;   // how long the pop-in takes
        public float overshoot = 1.15f;  // how much it overshoots (1.15 = 15% bigger then snaps back)
        public float jumpHeight = 0.15f;  // small upward bump on appear (the "jump" feel)

        private Vector3 originalPos;

        void Awake()
        {
            // Start invisible and tiny
            transform.localScale = Vector3.zero;
            originalPos = transform.localPosition;
        }

        void OnEnable()
        {
            StartCoroutine(AnimateIn());
        }

        IEnumerator AnimateIn()
        {
            float elapsed = 0f;
            Vector3 startPos = originalPos + Vector3.up * jumpHeight;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                float scale = SpringCurve(t, overshoot);
                transform.localScale = Vector3.one * scale;

                // Small position jump — block hops slightly upward then settles
                float posT = Mathf.SmoothStep(1f, 0f, t);
                transform.localPosition = Vector3.Lerp(originalPos, startPos, posT * (1f - t));

                yield return null;
            }

            // Snap to exact final state
            transform.localScale = Vector3.one;
            transform.localPosition = originalPos;
        }

        float SpringCurve(float t, float overshoot)
        {
            if (t < 0.6f)
            {
                return Mathf.Lerp(0f, overshoot, t / 0.6f);
            }
            else
            {
                return Mathf.Lerp(overshoot, 1f, (t - 0.6f) / 0.4f);
            }
        }
    }
}