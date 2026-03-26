using BeatSaberVR;
using DG.Tweening;
using UnityEngine;

public class RaysBehavior : MonoBehaviour
{
    [Header("Ray Objects")]
    [Tooltip("Drag all your individual Ray GameObjects (with MeshRenderers) here.")]
    [SerializeField] private Renderer[] rayRenderers;

    [Header("Colors")]
    [SerializeField, ColorUsage(true, true)] private Color redColor = Color.red;
    [SerializeField, ColorUsage(true, true)] private Color blueColor = Color.blue;

    [Header("Animation Settings")]
    public float fadeDuration = 0.15f;
    public float startAlpha = 0.8f;

    private Material[] rayMaterials;

    private static readonly int ColorProp = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionProp = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        rayMaterials = new Material[rayRenderers.Length];

        for (int i = 0; i < rayRenderers.Length; i++)
        {
            rayMaterials[i] = rayRenderers[i].material;
            SetRayAlpha(i, 0f);
            rayRenderers[i].gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        BeatSaberVREvents.OnBlockCut += ShowRay;
    }

    private void OnDisable()
    {
        BeatSaberVREvents.OnBlockCut -= ShowRay;

        for (int i = 0; i < rayRenderers.Length; i++)
        {
            if (rayRenderers[i] != null)
                DOTween.Kill(rayRenderers[i].transform);
        }
    }

    private void ShowRay(BlockColor color)
    {
        int availableIndex = GetAvailableRayIndex();

        // If all rays are currently flashing
        if (availableIndex == -1)
        {
            availableIndex = Random.Range(0, rayRenderers.Length);
        }

        Color targetColor = (color == BlockColor.Red) ? redColor : blueColor;

        rayMaterials[availableIndex].SetColor(ColorProp, targetColor);
        rayMaterials[availableIndex].SetColor(EmissionProp, targetColor);
        SetRayAlpha(availableIndex, startAlpha);

        rayRenderers[availableIndex].gameObject.SetActive(true);
        AnimateRay(availableIndex);
    }

    private int GetAvailableRayIndex()
    {
        for (int i = 0; i < rayRenderers.Length; i++)
        {
            if (!rayRenderers[i].gameObject.activeSelf)
            {
                return i;
            }
        }
        return -1;
    }

    private void AnimateRay(int index)
    {
        Transform rayTransform = rayRenderers[index].transform;

        DOTween.Kill(rayTransform);

        DOVirtual.Float(startAlpha, 0f, fadeDuration, (alpha) => SetRayAlpha(index, alpha))
            .SetEase((DG.Tweening.Ease)Ease.EaseOutCubic)
            .SetId(rayTransform)
            .OnComplete(() =>
            {
                rayRenderers[index].gameObject.SetActive(false);
            });
    }

    private void SetRayAlpha(int index, float alpha)
    {
        Material mat = rayMaterials[index];
        Color c = mat.GetColor(ColorProp);
        c.a = alpha;
        mat.SetColor(ColorProp, c);

        mat.SetColor(EmissionProp, c * Mathf.LinearToGammaSpace(alpha));
    }
}