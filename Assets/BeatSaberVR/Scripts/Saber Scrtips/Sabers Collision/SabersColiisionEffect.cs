using UnityEngine;

public class SabersColiisionEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem collisionEffect;
    [SerializeField] private AudioSource sfx;

    [Header("Saber Points")]
    [SerializeField] private Transform bladeBase;
    [SerializeField] private Transform bladeTip;

    private BoxCollider _myCollider;
    private bool isColliding = false;

    private void Awake()
    {
        _myCollider = GetComponent<BoxCollider>();
    }

    private void OnEnable()
    {
        if (collisionEffect != null) collisionEffect.Stop();
        if (sfx != null && sfx.isPlaying) sfx.Stop();
    }

    private void OnDisable()
    {
        if (collisionEffect != null) collisionEffect.Stop();
        if (sfx != null && sfx.isPlaying) sfx.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Saber")) return;
        if (!isColliding)
        {
            PlayCollisionEffect();
            isColliding = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Saber")) return;
        UpdateEffectPosition(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Saber")) return;
        StopCollisionEffect();
        isColliding = false;
    }

    private void PlayCollisionEffect()
    {
        if (collisionEffect != null) collisionEffect.Play();
        if (sfx != null && !sfx.isPlaying) sfx.Play();
    }

    private void StopCollisionEffect()
    {
        if (collisionEffect != null) collisionEffect.Stop();
        if (sfx != null && sfx.isPlaying) sfx.Stop();
    }

    private void UpdateEffectPosition(Collider other)
    {
        if (collisionEffect == null || bladeBase == null || bladeTip == null) return;

        SabersColiisionEffect otherSaberScript = other.GetComponent<SabersColiisionEffect>();
        if (otherSaberScript == null) return;

        Vector3 p1 = bladeBase.position;
        Vector3 q1 = bladeTip.position;

        Vector3 p2 = otherSaberScript.bladeBase.position;
        Vector3 q2 = otherSaberScript.bladeTip.position;

        CalculateClosestPoints(p1, q1, p2, q2, out Vector3 closestPointOnThis, out Vector3 closestPointOnOther);

        collisionEffect.transform.position = (closestPointOnThis + closestPointOnOther) * 0.5f;
    }

    //Standard Math for finding the closest points between two line segments
    private void CalculateClosestPoints(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2, out Vector3 c1, out Vector3 c2)
    {
        Vector3 d1 = q1 - p1;
        Vector3 d2 = q2 - p2;
        Vector3 r = p1 - p2;
        float a = Vector3.Dot(d1, d1);
        float e = Vector3.Dot(d2, d2);
        float f = Vector3.Dot(d2, r);

        float s, t;
        float c = Vector3.Dot(d1, r);
        float b = Vector3.Dot(d1, d2);
        float denom = a * e - b * b;

        if (denom != 0f)
        {
            s = Mathf.Clamp01((b * f - c * e) / denom);
        }
        else
        {
            s = 0f;
        }

        t = (b * s + f) / e;

        if (t < 0f)
        {
            t = 0f;
            s = Mathf.Clamp01(-c / a);
        }
        else if (t > 1f)
        {
            t = 1f;
            s = Mathf.Clamp01((b - c) / a);
        }
        else
        {
            t = Mathf.Clamp01(t);
        }

        c1 = p1 + d1 * s;
        c2 = p2 + d2 * t;
    }
}