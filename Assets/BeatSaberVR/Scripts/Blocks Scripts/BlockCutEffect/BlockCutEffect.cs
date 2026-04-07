using System.Collections;
using UnityEngine;

namespace BeatSaberVR
{
    public class BlockCutEffect : MonoBehaviour
    {
        [Header("Physics")]
        public float flyForce = 4f;
        public float upwardForce = 2f;
        public float spinForce = 300f;

        [Header("Fade")]
        public float lifetime = 0.6f;

        private Rigidbody rb;
        private Renderer[] renderers;
        private HalfType myType;
        private BlockColor myColor;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            renderers = GetComponentsInChildren<Renderer>();
        }

        public void Launch(Vector3 flyDirection, Vector3 saberVelocity,
                           HalfType type, BlockColor color)
        {
            myType = type;
            myColor = color;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 force = flyDirection * flyForce
                          + Vector3.up * upwardForce
                          + saberVelocity * 0.3f;

            rb.AddForce(force, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * spinForce, ForceMode.Force);

            StartCoroutine(FadeAndReturn());
        }

        IEnumerator FadeAndReturn()
        {
            Material[] mats = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                mats[i] = renderers[i].material;

            float elapsed = 0f;
            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / lifetime);
                foreach (var mat in mats)
                    if (mat.HasProperty("_BaseColor"))
                    {
                        Color c = mat.GetColor("_BaseColor");
                        c.a = alpha;
                        mat.SetColor("_BaseColor", c);
                    }
                yield return null;
            }

            // Reset alpha before returning to pool
            foreach (var mat in mats)
                if (mat.HasProperty("_BaseColor"))
                {
                    Color c = mat.GetColor("_BaseColor");
                    c.a = 1f;
                    mat.SetColor("_BaseColor", c);
                }

            if (PoolManager.Instance != null)
                PoolManager.Instance.ReturnHalf(this, myType, myColor);
        }
    }
}