using UnityEngine;

namespace BeatSaberVR
{
    public class WallBehavior : MonoBehaviour
    {
        [Header("Settings")]
        public float speed = 10f;
        public float height = 2.8f;
        public float depth = 0.5f;

        private const float HIT_COOLDOWN = 0.5f;

        private bool returned = false;
        private float lastHitTime = -99f;

        void OnEnable()
        {
            returned = false;
            lastHitTime = -99f;
        }

        void Update()
        {
            transform.Translate(Vector3.back * speed * Time.deltaTime);

            if (transform.position.z < -2f && !returned)
            {
                returned = true;
                BlockPoolManager.Instance.ReturnWall(this);
            }
        }

        public void Setup(float wallWidth, Vector3 position)
        {
            transform.position = position;
            transform.localScale = new Vector3(wallWidth, height, depth);
        }

        void OnTriggerEnter(Collider other)
        {
            HandlePlayerHit(other);
        }

        void OnTriggerStay(Collider other)
        {
            if (Time.time - lastHitTime >= HIT_COOLDOWN)
                HandlePlayerHit(other);
        }

        void HandlePlayerHit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            lastHitTime = Time.time;
            GameManager.Instance.RegisterWallHit();
        }
    }
}