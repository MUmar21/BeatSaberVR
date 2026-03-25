using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberVR
{
    public class SaberTrail : MonoBehaviour
    {
        [Header("Trail Points")]
        public Transform tipPoint;      // Top of blade (Tip GameObject)
        public Transform basePoint;     // Bottom of blade (Base GameObject)

        [Header("Trail Settings")]
        public float trailTime = 0.15f; // How long the trail lingers (seconds)
        public float minDistance = 0.02f; // Minimum movement required to add a point (Smooths the mesh)

        [Header("Material")]
        public Material trailMaterial;  // Requires a Particle Additive/Alpha Blended material

        private Mesh trailMesh;
        private GameObject trailObj;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        // Struct to store data for each slice of the trail
        private struct TrailPoint
        {
            public Vector3 tipPosition;
            public Vector3 basePosition;
            public float timeCreated;
        }

        private List<TrailPoint> points = new List<TrailPoint>();

        void Start()
        {
            // Create a separate GameObject for the mesh. 
            // We detach it so it doesn't inherit the saber's rotation/scale, keeping our world-space vertices accurate.
            trailObj = new GameObject("SaberTrailMesh");
            trailObj.transform.SetParent(null);
            trailObj.transform.position = Vector3.zero;
            trailObj.transform.rotation = Quaternion.identity;

            meshFilter = trailObj.AddComponent<MeshFilter>();
            meshRenderer = trailObj.AddComponent<MeshRenderer>();
            meshRenderer.material = trailMaterial;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            trailMesh = new Mesh();
            trailMesh.name = "TrailMeshDynamic";
            trailMesh.MarkDynamic(); // Optimizes the mesh for frequent updates
            meshFilter.mesh = trailMesh;
        }

        void Update()
        {
            UpdatePoints();
            BuildMesh();
        }

        void UpdatePoints()
        {
            float currentTime = Time.time;

            // 1. Remove points that have exceeded the trailTime
            while (points.Count > 0 && currentTime - points[0].timeCreated > trailTime)
            {
                points.RemoveAt(0);
            }

            // 2. Determine if we moved enough to warrant a new segment
            bool addPoint = false;
            if (points.Count == 0)
            {
                addPoint = true;
            }
            else
            {
                TrailPoint lastPoint = points[points.Count - 1];
                float distTip = Vector3.Distance(tipPoint.position, lastPoint.tipPosition);
                float distBase = Vector3.Distance(basePoint.position, lastPoint.basePosition);

                if (distTip > minDistance || distBase > minDistance)
                {
                    addPoint = true;
                }
            }

            // 3. Add new point or update the latest one for smoothness
            if (addPoint)
            {
                TrailPoint p = new TrailPoint();
                p.tipPosition = tipPoint.position;
                p.basePosition = basePoint.position;
                p.timeCreated = currentTime;
                points.Add(p);
            }
            else if (points.Count > 0)
            {
                // Constantly update the very last point to perfectly stick to the saber
                TrailPoint p = points[points.Count - 1];
                p.tipPosition = tipPoint.position;
                p.basePosition = basePoint.position;
                points[points.Count - 1] = p;
            }
        }

        void BuildMesh()
        {
            if (points.Count < 2)
            {
                trailMesh.Clear();
                return;
            }

            int numPoints = points.Count;
            Vector3[] vertices = new Vector3[numPoints * 2];
            Vector2[] uvs = new Vector2[numPoints * 2];
            Color[] colors = new Color[numPoints * 2];
            int[] triangles = new int[(numPoints - 1) * 6];

            float currentTime = Time.time;

            for (int i = 0; i < numPoints; i++)
            {
                TrailPoint p = points[i];

                // Vertices
                int baseIndex = i * 2;
                int tipIndex = i * 2 + 1;

                vertices[baseIndex] = p.basePosition;
                vertices[tipIndex] = p.tipPosition;

                // UV Mapping (X: 0=base, 1=tip | Y: 0=oldest tail, 1=newest head)
                float lengthPct = (float)i / (numPoints - 1);
                uvs[baseIndex] = new Vector2(0f, lengthPct);
                uvs[tipIndex] = new Vector2(1f, lengthPct);

                // Vertex Colors for fading out the tail
                float age = currentTime - p.timeCreated;
                float alpha = 1f - (age / trailTime);
                alpha = Mathf.Clamp01(alpha);

                Color vColor = new Color(1, 1, 1, alpha);
                colors[baseIndex] = vColor;
                colors[tipIndex] = vColor;

                // Triangles (Connecting the quads)
                if (i < numPoints - 1)
                {
                    int tIndex = i * 6;

                    triangles[tIndex] = baseIndex;
                    triangles[tIndex + 1] = tipIndex;
                    triangles[tIndex + 2] = baseIndex + 2;

                    triangles[tIndex + 3] = tipIndex;
                    triangles[tIndex + 4] = baseIndex + 3;
                    triangles[tIndex + 5] = baseIndex + 2;
                }
            }

            // Apply data to mesh
            trailMesh.Clear();
            trailMesh.vertices = vertices;
            trailMesh.uv = uvs;
            trailMesh.colors = colors;
            trailMesh.triangles = triangles;
        }

        // Clean up the detached mesh object when the sword is destroyed
        private void OnDestroy()
        {
            if (trailObj != null)
            {
                Destroy(trailObj);
            }
        }
    }
}