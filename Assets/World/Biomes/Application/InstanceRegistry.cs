using System.Collections.Generic;
using UnityEngine;

namespace Biomes.Application
{
    public static class InstanceRegistry
    {
        private class Entry
        {
            public GameObject prefab;
            public Mesh       mesh;
            public Material[] materials;
            public bool       allowInstancing;
            public Matrix4x4  localMatrix = Matrix4x4.identity;
            public Vector3    rootScale = Vector3.one;
            public float      groundOffset;
        }

        private static readonly Dictionary<int, Entry> _entries = new();

        public static void Register(GameObject prefab, bool allowInstancing = true)
        {
            if (prefab == null) return;

            int id = prefab.GetInstanceID();

            if (_entries.TryGetValue(id, out var existing))
            {
                existing.allowInstancing |= allowInstancing;
                return;
            }

            var e = new Entry
            {
                prefab          = prefab,
                allowInstancing = allowInstancing,
                rootScale = prefab.transform.localScale
            };

            MeshFilter mf = null;
            MeshRenderer mr = null;

            var renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var candidate = renderers[i];
                if (candidate == null)
                    continue;

                var candidateMf = candidate.GetComponent<MeshFilter>();
                if (candidateMf == null || candidateMf.sharedMesh == null)
                    continue;

                mr = candidate;
                mf = candidateMf;
                break;
            }

            if (mf != null && mr != null)
            {
                e.mesh      = mf.sharedMesh;
                e.materials = mr.sharedMaterials;
                e.localMatrix = prefab.transform.worldToLocalMatrix * mr.transform.localToWorldMatrix;
                e.groundOffset = ComputeGroundOffset(e.mesh, e.localMatrix);
            }

            _entries[id] = e;
        }

        public static bool TryGetPrefab(int id, out GameObject prefab)
        {
            if (_entries.TryGetValue(id, out var e) && e.prefab != null)
            {
                prefab = e.prefab;
                return true;
            }

            prefab = null;
            return false;
        }

        public static bool TryGetInstancedMesh(int id, out Mesh mesh, out Material[] materials)
        {
            if (_entries.TryGetValue(id, out var e) &&
                e.allowInstancing &&
                e.mesh != null &&
                e.materials != null &&
                e.materials.Length > 0)
            {
                mesh      = e.mesh;
                materials = e.materials;
                return true;
            }

            mesh      = null;
            materials = null;
            return false;
        }

        public static bool TryGetInstancedRenderData(
            int id,
            out Mesh mesh,
            out Material[] materials,
            out Matrix4x4 localMatrix,
            out Vector3 rootScale,
            out float groundOffset)
        {
            if (_entries.TryGetValue(id, out var e) &&
                e.allowInstancing &&
                e.mesh != null &&
                e.materials != null &&
                e.materials.Length > 0)
            {
                mesh = e.mesh;
                materials = e.materials;
                localMatrix = e.localMatrix;
                rootScale = e.rootScale;
                groundOffset = e.groundOffset;
                return true;
            }

            mesh = null;
            materials = null;
            localMatrix = Matrix4x4.identity;
            rootScale = Vector3.one;
            groundOffset = 0f;
            return false;
        }

        private static float ComputeGroundOffset(Mesh mesh, Matrix4x4 localMatrix)
        {
            if (mesh == null)
                return 0f;

            Bounds bounds = mesh.bounds;
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            Vector3[] corners =
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z)
            };

            float minY = float.PositiveInfinity;
            for (int i = 0; i < corners.Length; i++)
            {
                float y = localMatrix.MultiplyPoint3x4(corners[i]).y;
                if (y < minY)
                    minY = y;
            }

            return float.IsNaN(minY) || float.IsInfinity(minY) ? 0f : minY;
        }
    }
}
