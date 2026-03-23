using UnityEngine;
namespace BeatSaberVR
{
    public class SaberTrail : MonoBehaviour
    {

        [Header("Trail Points")]
        public Transform tipPoint;      // top of blade (Tip GameObject)
        public Transform basePoint;     // bottom of blade (create an empty at blade base)

        [Header("Trail Settings")]
        public int trailLength = 20;   // number of segments stored
        public float trailTime = 0.08f;

        [Header("Material")]
        public Material trailMaterial;    // your additive material

        private LineRenderer lineRenderer;
        private Vector3[] tipPositions;
        private Vector3[] basePositions;
        private float[] timestamps;
        private int head = 0;

        void Start()
        {
            tipPositions = new Vector3[trailLength];
            basePositions = new Vector3[trailLength];
            timestamps = new float[trailLength];

            // Create a LineRenderer as child for the ribbon
            GameObject lr = new GameObject("TrailMesh");
            lr.transform.SetParent(transform);
            lineRenderer = lr.AddComponent<LineRenderer>();
            lineRenderer.material = trailMaterial;
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
            lineRenderer.widthMultiplier = 0.04f;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        void Update()
        {
            // Record positions this frame
            tipPositions[head] = tipPoint.position;
            basePositions[head] = basePoint.position;
            timestamps[head] = Time.time;
            head = (head + 1) % trailLength;

            // Build the visible trail from recent points
            DrawTrail();
        }

        void DrawTrail()
        {
            float now = Time.time;
            int count = 0;

            // Count how many points are still within the trail time window
            for (int i = 0; i < trailLength; i++)
            {
                if (now - timestamps[i] < trailTime) count++;
            }

            lineRenderer.positionCount = count;
            int idx = 0;

            // Walk from oldest to newest — draw tip positions as the line
            for (int i = 0; i < trailLength; i++)
            {
                int pos = (head + i) % trailLength;
                if (now - timestamps[pos] < trailTime)
                {
                    lineRenderer.SetPosition(idx, tipPositions[pos]);

                    // Fade width older → newer
                    float age = (now - timestamps[pos]) / trailTime;
                    float width = Mathf.Lerp(0.04f, 0f, age);
                    // (Unity LineRenderer width per-vertex needs AnimationCurve — simpler: use widthMultiplier)

                    idx++;
                }
            }
        }
    }
}