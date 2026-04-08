using UnityEngine;

namespace BeatSaberVR
{
    public class VRHeadCollision : MonoBehaviour
    {
        private const float HIT_COOLDOWN = 1f;
        private float lastHitTime = -1f;

        private void OnTriggerEnter(Collider other)
        {
            HandlePlayerHit(other);
        }

        private void OnTriggerStay(Collider other)
        {
            HandlePlayerHit(other);
        }

        private void HandlePlayerHit(Collider other)
        {
            if (!other.CompareTag("Wall")) return;

            if (Time.unscaledTime - lastHitTime < HIT_COOLDOWN) return;

            lastHitTime = Time.unscaledTime;

            //GameManager.Instance.RegisterWallHit();
        }
    }
}