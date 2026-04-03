using UnityEngine;

namespace BeatSaberVR
{
    using DG.Tweening;

    public class RaysBehavior : MonoBehaviour
    {
        [Header("Ray Objects")]
        [SerializeField] private Renderer leftRayRenderer;
        [SerializeField] private Renderer rightRayRenderer;

        [Header("Colors")]
        [SerializeField, ColorUsage(true, true)] private Color redColor = Color.red;
        [SerializeField, ColorUsage(true, true)] private Color blueColor = Color.blue;

        [Header("Animation Settings")]
        public float fadeDuration = 0.15f;
        public float startAlpha = 0.8f;

        private Material leftMaterial;
        private Material rightMaterial;

        // Tracking for simultaneous cuts
        private int lastFrameProcessed = -1;
        private bool leftUsedThisFrame = false;
        private bool rightUsedThisFrame = false;

        private static readonly int ColorProp = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionProp = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            if (leftRayRenderer != null)
            {
                leftMaterial = leftRayRenderer.material;
                SetRayAlpha(leftMaterial, 0f);
            }

            if (rightRayRenderer != null)
            {
                rightMaterial = rightRayRenderer.material;
                SetRayAlpha(rightMaterial, 0f);
            }
        }

        private void OnEnable()
        {
            BeatSaberVREvents.OnBlockCut += ShowRay;
        }

        private void OnDisable()
        {
            BeatSaberVREvents.OnBlockCut -= ShowRay;
            if (leftRayRenderer != null)
                DOTween.Kill(leftRayRenderer?.transform);
            if (rightRayRenderer != null)
                DOTween.Kill(rightRayRenderer?.transform);
        }

        private void ShowRay(BlockColor color)
        {
            // Reset frame tracking for simultaneous block hits
            if (Time.frameCount != lastFrameProcessed)
            {
                lastFrameProcessed = Time.frameCount;
                leftUsedThisFrame = false;
                rightUsedThisFrame = false;
            }

            bool useLeft;

            // Logic: Balance sides during simultaneous cuts
            if (!leftUsedThisFrame && !rightUsedThisFrame)
            {
                useLeft = Random.value > 0.5f;
            }
            else if (leftUsedThisFrame)
            {
                useLeft = false;
            }
            else
            {
                useLeft = true;
            }

            if (useLeft)
            {
                AnimateRay(leftRayRenderer, leftMaterial, color, true);
                leftUsedThisFrame = true;
            }
            else
            {
                AnimateRay(rightRayRenderer, rightMaterial, color, false);
                rightUsedThisFrame = true;
            }
        }

        private void AnimateRay(Renderer renderer, Material mat, BlockColor color, bool isLeft)
        {
            if (renderer == null) return;

            float randomZ = Random.Range(25f, 75f);

            if (isLeft) randomZ *= -1f;

            renderer.transform.localRotation = Quaternion.Euler(0, 0, randomZ);

            Color targetColor = (color == BlockColor.Red) ? redColor : blueColor;
            mat.SetColor(ColorProp, targetColor);
            mat.SetColor(EmissionProp, targetColor);

            renderer.gameObject.SetActive(true);
            DOTween.Kill(renderer.transform);

            DOVirtual.Float(startAlpha, 0f, fadeDuration, (alpha) => SetRayAlpha(mat, alpha))
                .SetEase(Ease.OutCubic)
                .SetId(renderer.transform)
                .OnComplete(() => SetRayAlpha(mat, 0f));
        }

        private void SetRayAlpha(Material mat, float alpha)
        {
            Color c = mat.GetColor(ColorProp);
            c.a = alpha;
            mat.SetColor(ColorProp, c);
            mat.SetColor(EmissionProp, c * Mathf.LinearToGammaSpace(alpha));
        }
    }
}