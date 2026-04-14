using System.Collections;
using BeatSaberVR;
using DG.Tweening;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    [SerializeField] private Vector3 startPosition;

    [Header("Miss Effect Settings")]
    [SerializeField] private GameObject missEffect;
    [SerializeField] private float effectDuration = 0.3f;
    [SerializeField] private float targetScale = 1.5f;

    private MeshRenderer effectRenderer;
    private Material effectMaterial;
    private Vector3 initialEffectScale;

    private void Awake()
    {
        if (missEffect != null)
        {
            effectRenderer = missEffect.GetComponent<MeshRenderer>();
            effectMaterial = effectRenderer.material;
            initialEffectScale = missEffect.transform.localScale;

            // Start deactivated
            missEffect.SetActive(false);
        }
    }

    private void OnEnable()
    {
        BeatSaberVREvents.OnBlockMiss += PlayerMissEffect;
    }

    private void OnDisable()
    {
        BeatSaberVREvents.OnBlockMiss -= PlayerMissEffect;
    }

    IEnumerator Start()
    {
        yield return null;
        gameObject.transform.position = startPosition;
    }

    private void PlayerMissEffect()
    {
        if (missEffect == null) return;

        missEffect.SetActive(true);
        missEffect.transform.localScale = Vector3.zero;

        Sequence missSequence = DOTween.Sequence();

        Color baseColor = effectMaterial.GetColor("_EmissionColor");
        effectMaterial.SetColor("_EmissionColor", baseColor * 2f); // Make it 2x brighter

        missSequence.Join(effectMaterial.DOColor(baseColor, "_EmissionColor", effectDuration));
        missSequence.Append(missEffect.transform.DOScale(initialEffectScale * targetScale, effectDuration * 0.5f)
            .SetEase(Ease.OutBack));
        missSequence.Append(missEffect.transform.DOScale(Vector3.zero, effectDuration * 0.5f)
            .SetEase(Ease.InSine));

        missSequence.OnComplete(() => missEffect.SetActive(false));
    }
}