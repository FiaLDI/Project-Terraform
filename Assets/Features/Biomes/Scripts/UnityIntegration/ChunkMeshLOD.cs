using UnityEngine;

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
        private Transform player;
        private Mesh currentMesh;

        private void Awake()
        {
            mf = GetComponent<MeshFilter>();
        }

        private void Start()
        {
            FindPlayer();
            SetLODMesh(lod0Mesh);
        }

        private void Update()
        {
            if (player == null)
            {
                FindPlayer();
                if (player == null)
                    return;
            }

            float dist = Vector3.Distance(player.position, transform.position);

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

        private void FindPlayer()
        {
            if (RuntimeWorldGenerator.PlayerInstance != null)
                player = RuntimeWorldGenerator.PlayerInstance.transform;
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
