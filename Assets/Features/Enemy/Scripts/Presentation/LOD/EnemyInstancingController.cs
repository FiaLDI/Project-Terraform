using UnityEngine;

namespace Features.Enemy.Presentation.LOD
{
    [RequireComponent(typeof(EnemyLODView))]
    public class EnemyInstancingController : MonoBehaviour
    {
        private bool active;

        private EnemyLODView view;

        private Mesh mesh;
        private Material mat;

        private void Awake()
        {
            view = GetComponent<EnemyLODView>();
        }

        // =========================================================
        // PUBLIC API
        // =========================================================

        public void EnableInstancing()
        {
            if (active) return;
            active = true;

            // выключаем все LOD объекты
            for (int i = 0; i < 3; i++)
            {
                var obj = view.GetLODObject(i);
                if (obj != null)
                    obj.SetActive(false);
            }
        }

        public void DisableInstancing()
        {
            if (!active) return;
            active = false;

            var lod0 = view.GetLODObject(0);
            if (lod0 != null)
                lod0.SetActive(true);
                
            mesh = null;
            mat = null;
        }

        // =========================================================
        // RENDER
        // =========================================================

        private void LateUpdate()
        {
            if (!active) return;

            // если ещё не получили mesh/material — пробуем
            if (mesh == null || mat == null)
            {
                if (!TryResolveMesh())
                    return;
            }

            if (mesh == null || mat == null)
                return;

            // ❗ instancing должен быть включен в материале
            if (!mat.enableInstancing)
                return;

            EnemyGPUInstancer.Instance?.AddInstance(
                mesh,
                mat,
                new EnemyInstance
                {
                    position = transform.position,
                    rotation = transform.rotation,
                    scale = transform.lossyScale.x,
                    color = Color.white
                },
                UnityEngine.Rendering.ShadowCastingMode.On,
                true,
                gameObject.layer
            );
        }

        // =========================================================
        // INTERNAL
        // =========================================================

        private bool TryResolveMesh()
        {
            var obj = view.GetLODObject(2); // LOD2 = instancing
            if (obj == null)
                return false;

            var renderer = obj.GetComponentInChildren<Renderer>();
            if (renderer == null)
                return false;

            // ===== MeshRenderer (обычный меш)
            if (renderer is MeshRenderer)
            {
                var mf = renderer.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null)
                {
                    Debug.LogWarning("[Instancing] MeshFilter missing on LOD2", this);
                    return false;
                }

                mesh = mf.sharedMesh;
            }
            // ===== SkinnedMeshRenderer (fallback)
            else if (renderer is SkinnedMeshRenderer skinned)
            {
                if (skinned.sharedMesh == null)
                {
                    Debug.LogWarning("[Instancing] Skinned mesh is null", this);
                    return false;
                }

                mesh = skinned.sharedMesh;
            }
            else
            {
                Debug.LogWarning("[Instancing] Unsupported renderer type", this);
                return false;
            }

            mat = renderer.sharedMaterial;

            if (mesh == null || mat == null)
            {
                Debug.LogWarning("[Instancing] Failed to resolve mesh/material", this);
                return false;
            }

            return true;
        }
    }
}
