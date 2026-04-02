using UnityEngine;
namespace BeatSaberVR
{
    [System.Serializable]
    public struct SwordData
    {
        public Swords sword;
        public GameObject gameObject;
    }

    public enum Swords
    {
        SwordA, SwordB
    }

    public class SaberController : MonoBehaviour
    {
        public SwordData[] swordDatas;

        [Header("Identity")]
        public BlockColor saberColor;   // set Left on left saber, Right on right saber

        public Vector3 velocity;
        [HideInInspector] public Vector3 swingDirection;

        private Vector3 lastPosition;
        private Vector3 lastSwingDir;

        private void OnEnable()
        {
            BeatSaberVREvents.OnSwordSelected += OnSwordSelected;

        }

        private void OnDisable()
        {
            BeatSaberVREvents.OnSwordSelected -= OnSwordSelected;
        }

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

        private void OnSwordSelected(Swords swords)
        {
            if (swordDatas == null || swordDatas.Length == 0) return;

            for (int i = 0; i < swordDatas.Length; i++)
            {
                if (swordDatas[i].gameObject != null && swordDatas[i].sword == swords)
                    swordDatas[i].gameObject.SetActive(true);
                else
                    swordDatas[i].gameObject.SetActive(false);
            }
        }

    }
}