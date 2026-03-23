using UnityEngine;
namespace BeatSaberVR
{
    public class SaberController : MonoBehaviour
    {
        [Header("Identity")]
        public BlockColor saberColor;   // set Left on left saber, Right on right saber

        [Header("Trail")]
        public TrailRenderer trail;     // drag the TrailRenderer component here

        // These are read by BlockBehavior during slash detection
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
            // Calculate velocity from position change this frame
            velocity = (transform.position - lastPosition) / Time.deltaTime;
            swingDirection = velocity.normalized;
            lastPosition = transform.position;

            // Smooth the swing direction so jitter doesn't cause false misses
            lastSwingDir = Vector3.Lerp(lastSwingDir, swingDirection, 0.3f);
            swingDirection = lastSwingDir;
        }

        // Speed shortcut used in slash check
        public float Speed => velocity.magnitude;
    }
}