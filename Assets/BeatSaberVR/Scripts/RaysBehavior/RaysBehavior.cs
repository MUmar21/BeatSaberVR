using BeatSaberVR;
using DG.Tweening;
using UnityEngine;

public class RaysBehavior : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField, ColorUsage(true, true)] private Color redColor = Color.red;
    [SerializeField, ColorUsage(true, true)] private Color blueColor = Color.blue;

    [Header("Animation Settings")]
    public float fadeDuration = 0.15f;
    public float startAlpha = 0.8f;

    private Material rayMaterial;
    private static readonly int ColorProp = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionProp = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        rayMaterial = GetComponent<Renderer>().material;
        SetAlpha(0);
    }

    private void OnEnable()
    {
        BeatSaberVREvents.OnBlockCut += ShowRay;
    }

    private void OnDisable()
    {
        BeatSaberVREvents.OnBlockCut -= ShowRay;
        DOTween.Kill(this.transform);
    }

    private void ShowRay(BlockColor color)
    {
        Color targetColor = (color == BlockColor.Red) ? redColor : blueColor;

        rayMaterial.SetColor(ColorProp, targetColor);
        rayMaterial.SetColor(EmissionProp, targetColor);
        SetAlpha(startAlpha);

        Animate();
    }

    private void Animate()
    {
        DOTween.Kill(this.transform);

        DOVirtual.Float(startAlpha, 0f, fadeDuration, SetAlpha)
            .SetEase((DG.Tweening.Ease)Ease.EaseInCubic)
            .SetId(this.transform);
    }

    private void SetAlpha(float alpha)
    {
        Color c = rayMaterial.GetColor(ColorProp);
        c.a = alpha;
        rayMaterial.SetColor(ColorProp, c);

        rayMaterial.SetColor(EmissionProp, c * Mathf.LinearToGammaSpace(alpha));
    }
}