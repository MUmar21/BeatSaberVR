// WallBehavior.cs — full script
using UnityEngine;

namespace BeatSaberVR
{
    public class WallBehavior : MonoBehaviour
    {
        [Header("Settings")]
        public float speed = 10f;   // match block speed
        public float height = 2.8f;  // full player height
        public float depth = 0.6f;  // wall thickness

        private bool returned = false;

        void OnEnable()
        {
            returned = false;
        }

        void Update()
        {
            transform.Translate(Vector3.back * speed * Time.deltaTime);

            // Return to pool once it passes the player
            if (transform.position.z < -2f && !returned)
            {
                returned = true;
                BlockPoolManager.Instance.ReturnWall(this);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.RegisterWallHit();
            }
        }

        public void Setup(float wallWidth, Vector3 position)
        {
            transform.position = position;
            transform.localScale = new Vector3(wallWidth, height, depth);
        }
    }
}