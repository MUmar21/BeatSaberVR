using System.Collections;
using UnityEngine;

namespace BeatSaberVR
{
    public class BlockAppearAnimation : MonoBehaviour
    {
        [Header("Timing")]
        public float duration = 0.25f;   // fast — Beat Saber is snappy

        [Header("Scale spring")]
        public float overshoot = 1.2f;    // 20% bigger before snapping to 1

        [Header("Shoot effect")]
        public float shootDistance = 0.8f;    // how far behind spawn point it starts
        public float shootCurve = 3f;       // how quickly it shoots forward (higher = snappier)

        [Header("Rotation tumble")]
        public float startTiltX = 25f;      // degrees tilted on X at spawn
        public float startTiltZ = 15f;      // slight Z tilt too

        private Coroutine currentAnim;
        private Vector3 targetPos;

        void OnEnable()
        {
            if (currentAnim != null) StopCoroutine(currentAnim);

            targetPos = transform.position;

            transform.localScale = Vector3.zero;
            transform.localRotation = Quaternion.Euler(startTiltX, 0f, startTiltZ);
            transform.position = targetPos + Vector3.back * shootDistance;

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
            transform.localRotation = Quaternion.identity;
        }

        IEnumerator AnimateIn()
        {
            float elapsed = 0f;

            Vector3 startPos = targetPos + Vector3.back * shootDistance;
            Quaternion startRot = Quaternion.Euler(startTiltX, 0f, startTiltZ);
            Quaternion endRot = Quaternion.identity;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // ── Scale: spring overshoot ────────────────────
                transform.localScale = Vector3.one * ScaleSpring(t);

                // ── Position: fast exponential shoot forward ───
                // EaseOutExpo makes it shoot fast then settle
                float posT = EaseOutExpo(t);
                transform.position = Vector3.Lerp(startPos, targetPos, posT);

                // ── Rotation: snap from tilted to straight ─────
                // Slightly behind the scale so it feels like
                // it tumbles into position
                float rotT = Mathf.SmoothStep(0f, 1f, t * 1.2f);
                transform.localRotation = Quaternion.Slerp(startRot, endRot,
                                                           Mathf.Clamp01(rotT));

                yield return null;
            }

            // Snap to exact final state
            transform.localScale = Vector3.one;
            transform.localRotation = endRot;
            transform.position = targetPos;
            currentAnim = null;
        }

        // Scale spring: 0 → overshoot → 1
        float ScaleSpring(float t)
        {
            if (t < 0.55f)
                return Mathf.Lerp(0f, overshoot, EaseOutCubic(t / 0.55f));
            else
                return Mathf.Lerp(overshoot, 1f, EaseOutCubic((t - 0.55f) / 0.45f));
        }

        // Shoots fast, decelerates to rest — the "pop" feel
        float EaseOutExpo(float t)
        {
            return t >= 1f ? 1f : 1f - Mathf.Pow(2f, -shootCurve * 10f * t);
        }

        float EaseOutCubic(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }
    }
}