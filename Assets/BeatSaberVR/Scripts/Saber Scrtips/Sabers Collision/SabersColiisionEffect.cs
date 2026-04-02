using UnityEngine;

public class SabersColiisionEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem collisionEffect;
    [SerializeField] private AudioSource sfx;
    private bool isColliding = false;

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
        if (collisionEffect == null) return;

        Collider thisCollider = GetComponent<Collider>();
        if (thisCollider == null) return;

        Vector3 guessPoint = other.bounds.center;
        Vector3 pointOnThis = thisCollider.ClosestPoint(guessPoint);
        Vector3 pointOnOther = other.ClosestPoint(pointOnThis);
        Vector3 contactPoint = (pointOnThis + pointOnOther) * 0.5f;
        collisionEffect.transform.position = contactPoint;
    }
}