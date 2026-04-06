using TMPro;
using UnityEngine;

namespace BeatSaberVR
{
    public class BlockBehavior : MonoBehaviour
    {
        [Header("References")]
        public Transform directionPoint;
        public GameObject defaultPoint;
        public ChoiceData choiceData;
        public TMP_Text choiceText;

        [Header("Block Settings")]
        public BlockColor blockColor;
        public CutDirection cutDirection;
        public float speed = 10f;

        private bool wasHit = false;
        private const float MIN_SWING_SPEED = 1.5f;

        [Header("Editor Testing Mouse MIN_SWING_SPEED")]
        [SerializeField] private float minSwingSpeed = 0.5f;

        private void OnEnable()
        {
            wasHit = false;
        }

        private void Update()
        {
            transform.Translate(Vector3.back * speed * Time.deltaTime);

            if (transform.position.z < -1.5f && !wasHit)
            {
                GameManager.Instance.RegisterMiss();
                ReturnToPool();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (wasHit) return;

            SaberController saber = other.GetComponentInParent<SaberController>();
            if (saber == null)
            {
                Debug.Log("Saber Is NULL Returning!!!");
                return;
            }

            bool colorMatch = saber.saberColor == blockColor;
            bool fastEnough;

            if (Application.isEditor) fastEnough = saber.Speed > minSwingSpeed;
            else fastEnough = saber.Speed > MIN_SWING_SPEED; // Must be swinging fast enough — not just resting on the block

            bool directionCorrect = CheckDirection(saber.swingDirection);

            if (colorMatch && fastEnough && directionCorrect)
            {
                OnGoodHit(saber);
            }
            else
            {
                OnBadHit(colorMatch, fastEnough, directionCorrect);
            }
        }

        private bool CheckDirection(Vector3 swingDir)
        {
            if (cutDirection == CutDirection.Any) return true;

            Vector3 expected = GetExpectedDirection();

            // Check if the swing direction is close enough to expected
            // Dot product > 0.5 means within ~60 degrees
            float dot = Vector3.Dot(swingDir.normalized, expected.normalized);
            return dot > 0.5f;
        }

        private Vector3 GetExpectedDirection()
        {
            return cutDirection switch
            {
                CutDirection.Up => Vector3.up,
                CutDirection.Down => Vector3.down,
                CutDirection.Left => Vector3.left,
                CutDirection.Right => Vector3.right,
                _ => Vector3.up
            };
        }

        private void OnGoodHit(SaberController saber)
        {
            wasHit = true;
            GameManager.Instance.AddScore(100);
            AudioManager.Instance.PlaySlash();
            PoolManager.Instance.PlayCutParticle(transform.position, blockColor);
            if (PlayerFinanceManager.Instance != null && choiceData != null)
                PlayerFinanceManager.Instance.ProcessChoice(choiceData, blockColor);
            BeatSaberVREvents.OnBlockCut?.Invoke(blockColor);

            SpawnCutPieces(saber.velocity);
            ReturnToPool();
        }

        private void OnBadHit(bool color, bool speed, bool dir)
        {
            if (!color) Debug.Log("Miss: Wrong saber color");
            if (!speed) Debug.Log("Miss: Swung too slowly");
            if (!dir) Debug.Log("Miss: Wrong direction");

            GameManager.Instance.RegisterMiss();
            ReturnToPool();
        }

        private void SpawnCutPieces(Vector3 saberVelocity)
        {
            HalfType typeA, typeB;
            Vector3 dirA, dirB;

            switch (cutDirection)
            {
                case CutDirection.Up:
                case CutDirection.Down:
                    typeA = HalfType.Left; dirA = Vector3.left;
                    typeB = HalfType.Right; dirB = Vector3.right;
                    break;
                case CutDirection.Left:
                case CutDirection.Right:
                    typeA = HalfType.Top; dirA = Vector3.up;
                    typeB = HalfType.Bottom; dirB = Vector3.down;
                    break;
                default: // Any — dot block
                    if (Mathf.Abs(saberVelocity.y) > Mathf.Abs(saberVelocity.x))
                    {
                        typeA = HalfType.Left; dirA = Vector3.left;
                        typeB = HalfType.Right; dirB = Vector3.right;
                    }
                    else
                    {
                        typeA = HalfType.Top; dirA = Vector3.up;
                        typeB = HalfType.Bottom; dirB = Vector3.down;
                    }
                    break;
            }

            BlockCutEffect halfA = PoolManager.Instance.GetHalf(typeA, blockColor);
            BlockCutEffect halfB = PoolManager.Instance.GetHalf(typeB, blockColor);

            if (halfA == null || halfB == null) return;

            halfA.transform.SetPositionAndRotation(transform.position, transform.rotation);
            halfB.transform.SetPositionAndRotation(transform.position, transform.rotation);

            halfA.Launch(dirA, saberVelocity, typeA, blockColor);
            halfB.Launch(dirB, saberVelocity, typeB, blockColor);
        }

        public void SetDirectionPoint(CutDirection cutDirection)
        {
            if (directionPoint == null) return;

            defaultPoint.SetActive(false);

            float zAngle = 0f;
            switch (cutDirection)
            {
                case CutDirection.Up:
                    zAngle = 0f;
                    defaultPoint.SetActive(false);
                    directionPoint.gameObject.SetActive(true);
                    break;
                case CutDirection.Down:
                    zAngle = 180f;
                    defaultPoint.SetActive(false);
                    directionPoint.gameObject.SetActive(true);
                    break;
                case CutDirection.Left:
                    zAngle = 90f;
                    defaultPoint.SetActive(false);
                    directionPoint.gameObject.SetActive(true);
                    break;
                case CutDirection.Right:
                    zAngle = 270f;
                    defaultPoint.SetActive(false);
                    directionPoint.gameObject.SetActive(true);
                    break;
                case CutDirection.Any:
                    defaultPoint.SetActive(true);
                    directionPoint.gameObject.SetActive(false);
                    break;
                default:
                    defaultPoint.SetActive(true);
                    directionPoint.gameObject.SetActive(false);
                    break;
            }

            directionPoint.localRotation = Quaternion.Euler(0f, 0f, zAngle);
        }

        private void ReturnToPool()
        {
            PoolManager.Instance.ReturnBlock(this);
        }

        public void ResetState()
        {
            wasHit = false;
            transform.localScale = Vector3.one;
        }
    }
}