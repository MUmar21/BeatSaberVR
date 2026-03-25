using UnityEngine;
namespace BeatSaberVR
{
    public class SaberController : MonoBehaviour
    {
        [Header("Identity")]
        public BlockColor saberColor;   // set Left on left saber, Right on right saber

        public Vector3 velocity;
        [HideInInspector] public Vector3 swingDirection;

        private Vector3 lastPosition;
        private Vector3 lastSwingDir;

        void Start()
        {
            lastPosition = transform.position;
        }

        void Update()
        {
            velocity = (transform.position - lastPosition) / Time.deltaTime;
            swingDirection = velocity.normalized;
            lastPosition = transform.position;

            // Smooth the swing direction so jitter doesn't cause false misses
            lastSwingDir = Vector3.Lerp(lastSwingDir, swingDirection, 0.3f);
            swingDirection = lastSwingDir;
        }

        public float Speed => velocity.magnitude;
    }
}