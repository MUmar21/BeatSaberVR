using UnityEngine;

namespace BeatSaberVR
{
    public class WallBehavior : MonoBehaviour
    {
        [Header("Settings")]
        public float speed = 10f;
        public float height = 2.8f;
        public float depth = 0.5f;

        private bool returned = false;

        void OnEnable()
        {
            returned = false;
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
    }
}