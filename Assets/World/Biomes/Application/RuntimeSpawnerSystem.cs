using UnityEngine;
using Unity.Mathematics;
using FishNet;
using FishNet.Object;
using Biomes.Domain;

namespace Biomes.Application
{
    /// <summary>
    /// Спавн обычных GameObject (ресурсы, враги, квесты) по данным SpawnInstance.
    /// </summary>
    public static class RuntimeSpawnerSystem
    {
        private static bool IsFinite(float v) =>
            !float.IsNaN(v) && !float.IsInfinity(v);

        private static bool IsFinite(float3 v) =>
            IsFinite(v.x) && IsFinite(v.y) && IsFinite(v.z);

        public static void SpawnObject(SpawnInstance inst, Vector2Int chunk, Transform parent)
        {
            if (!InstanceFinder.IsServer)
                return;

            if (!IsFinite(inst.position))
                return;

            if (!InstanceRegistry.TryGetPrefab(inst.prefabIndex, out var prefab))
                return;

            Vector3 pos = new Vector3(inst.position.x, inst.position.y, inst.position.z);
            Vector3 normalVec = new Vector3(inst.normal.x, inst.normal.y, inst.normal.z);

            Quaternion rotation =
                normalVec.sqrMagnitude > 0.0001f
                ? Quaternion.FromToRotation(Vector3.up, normalVec)
                : Quaternion.identity;

            GameObject go;

            var nobPrefab = prefab.GetComponent<NetworkObject>();

            if (nobPrefab != null)
            {
                go = Object.Instantiate(prefab, pos, rotation);

                var nob = go.GetComponent<NetworkObject>();
                InstanceFinder.ServerManager.Spawn(nob);
            }
            else
            {
                go = Object.Instantiate(prefab, pos, rotation);
            }

            if (parent != null)
                go.transform.SetParent(parent, true);

            go.transform.localScale = Vector3.one * inst.scale;

            SnapToGroundIgnoringSelf(go.transform, ref pos, ref rotation);
            go.transform.SetPositionAndRotation(pos, rotation);

            ChunkedGameObjectStorage.Register(chunk, go);
        }

        private static void SnapToGroundIgnoringSelf(Transform tr, ref Vector3 pos, ref Quaternion rot)
        {
            int originalLayer = tr.gameObject.layer;

            tr.gameObject.layer = 2;

            Vector3 origin = pos + Vector3.up * 10f;

            if (Physics.Raycast(
                    origin,
                    Vector3.down,
                    out RaycastHit hit,
                    200f,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore))
            {
                pos = hit.point;
                Vector3 n = hit.normal.sqrMagnitude > 0.0001f ? hit.normal : Vector3.up;
                float yaw = rot.eulerAngles.y;
                Quaternion basis = Quaternion.FromToRotation(Vector3.up, n);
                rot = basis * Quaternion.Euler(0f, yaw, 0f);
            }

            tr.gameObject.layer = originalLayer;
        }

    }
}
