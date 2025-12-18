using System.Collections.Generic;
using UnityEngine;

namespace Features.Biomes.Application
{
    public static class InstanceRegistry
    {
        private class Entry
        {
            public GameObject prefab;
            public Mesh       mesh;
            public Material[] materials;
            public bool       allowInstancing;
        }

        private static readonly Dictionary<int, Entry> _entries = new();

        public static void Register(GameObject prefab, bool allowInstancing = true)
        {
            if (prefab == null) return;

            int id = prefab.GetInstanceID();

            if (_entries.TryGetValue(id, out var existing))
            {
                // если когда-то уже регали как instanced = true, не затираем на false
                existing.allowInstancing |= allowInstancing;
                return;
            }

            var e = new Entry
            {
                prefab          = prefab,
                allowInstancing = allowInstancing
            };

            var mf = prefab.GetComponentInChildren<MeshFilter>();
            var mr = prefab.GetComponentInChildren<MeshRenderer>();

            if (mf != null && mr != null)
            {
                e.mesh      = mf.sharedMesh;
                e.materials = mr.sharedMaterials;
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
    }
}
