using UnityEngine;

namespace Features.Biomes.UnityIntegration
{
    public class ChunkColliderLODFixed : MonoBehaviour
    {
        [Header("Mesh used for physics")]
        public Mesh physicsMesh; // должен быть назначен ДО Start()

        private MeshCollider col;

        private void Awake()
        {
            col = GetComponent<MeshCollider>();
        }

        private void Start()
        {
            if (physicsMesh == null)
            {
                Debug.LogError($"[ChunkColliderLODFixed] physicsMesh is NULL on {gameObject.name}");
                return;
            }

            col.sharedMesh = null;       // нужно чтобы Unity пересоздал BVH
            col.sharedMesh = physicsMesh;
        }
    }
}