using UnityEngine;

namespace BeatSaberVR
{
    public class BlockBehavior : MonoBehaviour
    {
        [Header("Block Settings")]
        public BlockColor blockColor;
        public CutDirection cutDirection;
        public Transform directionPoint;
        public float speed = 10f;

        private bool wasHit = false;

        // Minimum saber speed to count as a real swing (not accidental graze)
        private const float MIN_SWING_SPEED = 0f;

        void Update()
        {
            transform.Translate(Vector3.back * speed * Time.deltaTime);

            // Block passed the player without being hit = miss
            if (transform.position.z < -1.5f && !wasHit)
            {
                GameManager.Instance.RegisterMiss();
                Destroy(gameObject);
            }
        }

        // Unity calls this automatically when a trigger collider touches us
        void OnTriggerEnter(Collider other)
        {
            Debug.Log($"HIT {other.gameObject.name}");

            if (wasHit) return; // already hit, ignore extra triggers

            // The Tip object has the collider — get SaberController from its parent
            SaberController saber = other.GetComponentInParent<SaberController>();
            if (saber == null)
            {
                Debug.Log("Saber Is NULL Returning!!!");
                return;
            }

            // ── Check 1: Color match ──────────────────────
            // Left saber (red) must hit red blocks
            // Right saber (blue) must hit blue blocks
            bool colorMatch = saber.saberColor == blockColor;

            // ── Check 2: Swing speed ──────────────────────
            // Must be swinging fast enough — not just resting on the block
            bool fastEnough = saber.Speed > MIN_SWING_SPEED;

            // ── Check 3: Cut direction ────────────────────
            bool directionCorrect = CheckDirection(saber.swingDirection);

            // ── Result ────────────────────────────────────
            if (colorMatch && fastEnough && directionCorrect)
            {
                OnGoodHit(saber);
            }
            else
            {
                OnBadHit(colorMatch, fastEnough, directionCorrect);
            }
        }

        bool CheckDirection(Vector3 swingDir)
        {
            // Dot blocks (Any) always pass
            if (cutDirection == CutDirection.Any) return true;

            // Get what direction the block expects
            Vector3 expected = GetExpectedDirection();

            // Check if the swing direction is close enough to expected
            // Dot product > 0.5 means within ~60 degrees — forgiving for beginners
            float dot = Vector3.Dot(swingDir.normalized, expected.normalized);
            return dot > 0.5f;
        }

        Vector3 GetExpectedDirection()
        {
            // These are in world space — the block flies toward z=0
            return cutDirection switch
            {
                CutDirection.Up => Vector3.up,
                CutDirection.Down => Vector3.down,
                CutDirection.Left => Vector3.left,
                CutDirection.Right => Vector3.right,
                _ => Vector3.up
            };
        }

        void OnGoodHit(SaberController saber)
        {
            wasHit = true;

            // Add score
            GameManager.Instance.AddScore(100);

            // Spawn particles at hit position
            //VFXManager.Instance.PlayHitEffect(transform.position, blockColor);

            // Play slash sound
            AudioManager.Instance.PlaySlash();

            Destroy(gameObject);
        }

        void OnBadHit(bool color, bool speed, bool dir)
        {
            // Log what failed — helpful while testing
            if (!color) Debug.Log("Miss: Wrong saber color");
            if (!speed) Debug.Log("Miss: Swung too slowly");
            if (!dir) Debug.Log("Miss: Wrong direction");

            GameManager.Instance.RegisterMiss();
            Destroy(gameObject);
        }
        public void SetDirectionPoint(CutDirection cutDirection)
        {
            if (directionPoint == null) return;

            float zAngle;
            switch (cutDirection)
            {
                case CutDirection.Up:
                    zAngle = 0f;
                    break;
                case CutDirection.Down:
                    zAngle = 180f;
                    break;
                case CutDirection.Left:
                    zAngle = 90f;
                    break;
                case CutDirection.Right:
                    zAngle = 270f;
                    break;
                case CutDirection.Any:
                default:
                    zAngle = directionPoint.localEulerAngles.z;
                    break;
            }

            directionPoint.localRotation = Quaternion.Euler(0f, 0f, zAngle);
        }
    }
}