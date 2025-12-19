using UnityEngine;
using Features.Player.UnityIntegration;

namespace Features.Biomes.UnityIntegration
{
    public class ChunkMeshLOD : MonoBehaviour
    {
        [Header("LOD Distances")]
        public float lod1Distance = 80f;
        public float lod2Distance = 160f;

        [Header("Meshes (Render only)")]
        public Mesh lod0Mesh;
        public Mesh lod1Mesh;
        public Mesh lod2Mesh;

        private MeshFilter mf;
        private Mesh currentMesh;

        private void Awake()
        {
            mf = GetComponent<MeshFilter>();
            SetLODMesh(lod0Mesh);
        }

        private void Update()
        {
            var registry = PlayerRegistry.Instance;
            if (registry == null || registry.LocalPlayer == null)
                return;

            Vector3 playerPos = registry.LocalPlayer.transform.position;
            float dist = Vector3.Distance(playerPos, transform.position);

            if (dist > lod2Distance)
            {
                if (currentMesh != lod2Mesh)
                    SetLODMesh(lod2Mesh);
            }
            else if (dist > lod1Distance)
            {
                if (currentMesh != lod1Mesh)
                    SetLODMesh(lod1Mesh);
            }
            else
            {
                if (currentMesh != lod0Mesh)
                    SetLODMesh(lod0Mesh);
            }
        }

        private void SetLODMesh(Mesh m)
        {
            if (m == null || mf == null)
                return;

            currentMesh = m;
            mf.sharedMesh = m;
        }
    }
}
