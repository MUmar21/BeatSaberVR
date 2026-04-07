using TMPro;
using UnityEngine;

namespace BeatSaberVR
{
    public class BlockBehavior : MonoBehaviour
    {
        [Header("References")]
        public Transform directionPoint;
        public GameObject defaultPoint;
        public TMP_Text choiceText;

        public ChoiceData Data { get; set; }

        [Header("Block Settings")]
        public BlockColor blockColor;
        public CutDirection cutDirection;

        private bool wasHit = false;
        private const float MIN_SWING_SPEED = 1.5f;

        [Header("Editor Testing Mouse MIN_SWING_SPEED")]
        [SerializeField] private float minSwingSpeed = 0.5f;

        private BlockAppearAnimation appearAnimation;

        private void Awake()
        {
            appearAnimation = GetComponent<BlockAppearAnimation>();
            if (appearAnimation == null)
            {
                Debug.LogWarning("[BlockBehavior] BlockAppearAnimation component is missing!");
            }
        }

        private void OnEnable()
        {
            wasHit = false;
            BeatSaberVREvents.OnChoiceMade += HandleChoiceMade;
        }
        private void OnDisable()
        {
            BeatSaberVREvents.OnChoiceMade -= HandleChoiceMade;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (wasHit) return;

            SaberController saber = other.GetComponentInParent<SaberController>();
            if (saber == null)
            {
                Debug.LogWarning("[BlockBehavior] Saber Is NULL Returning!!!");
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

        private void OnGoodHit(SaberController saber)
        {
            wasHit = true;

            if (GameManager.Instance == null || AudioManager.Instance == null || PoolManager.Instance == null || PlayerFinanceManager.Instance == null)
            {
                Debug.LogError("[BlockBehavior] One or more manager instances are missing! Cannot process good hit.");
                return;
            }

            GameManager.Instance.AddScore(100);
            AudioManager.Instance.PlaySlash();
            PoolManager.Instance.PlayCutParticle(transform.position, blockColor);
            if (Data != null) PlayerFinanceManager.Instance.ProcessChoice(Data, blockColor);

            BeatSaberVREvents.OnBlockCut?.Invoke(blockColor);

            if (appearAnimation != null)
            {
                Vector3 velocity = saber.velocity;
                appearAnimation.PlayPopOut(() =>
                {
                    SpawnCutPieces(velocity);
                    ReturnToPool();
                });
            }
            else
            {
                SpawnCutPieces(saber.velocity);
                ReturnToPool();
            }
        }

        private void OnBadHit(bool color, bool speed, bool dir)
        {
            wasHit = true;

            if (!color) Debug.Log("Miss: Wrong saber color");
            if (!speed) Debug.Log("Miss: Swung too slowly");
            if (!dir) Debug.Log("Miss: Wrong direction");

            if (GameManager.Instance != null)
                GameManager.Instance.RegisterMiss();

            BeatSaberVREvents.OnChoiceMade?.Invoke();

            if (appearAnimation != null)
                appearAnimation.PlayPopOut(ReturnToPool);
            else
                ReturnToPool();
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

        public void SetDirectionPoint(CutDirection dir)
        {
            cutDirection = dir;
            if (directionPoint == null) return;

            bool isAny = dir == CutDirection.Any;
            defaultPoint.SetActive(isAny);
            directionPoint.gameObject.SetActive(!isAny);

            if (!isAny)
            {
                float zAngle = dir switch
                {
                    CutDirection.Up => 0f,
                    CutDirection.Down => 180f,
                    CutDirection.Left => 90f,
                    CutDirection.Right => 270f,
                    _ => 0f
                };
                directionPoint.localRotation = Quaternion.Euler(0f, 0f, zAngle);
            }
        }

        private void HandleChoiceMade()
        {
            if (wasHit) return;

            wasHit = true;
            if (appearAnimation != null)
                appearAnimation.PlayPopOut(ReturnToPool);
            else
                ReturnToPool();
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