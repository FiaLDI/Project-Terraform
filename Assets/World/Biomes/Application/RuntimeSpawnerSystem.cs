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
            float safeScale = inst.scale <= 0f ? 1f : inst.scale;

            Quaternion rotation =
                normalVec.sqrMagnitude > 0.0001f
                ? Quaternion.FromToRotation(Vector3.up, normalVec)
                : Quaternion.identity;

            SnapToGround(ref pos, ref rotation);

            var nobPrefab = prefab.GetComponent<NetworkObject>();
            GameObject go = Object.Instantiate(prefab, pos, rotation);
            go.transform.localScale = Vector3.Scale(prefab.transform.localScale, Vector3.one * safeScale);

            if (nobPrefab == null && parent != null)
                go.transform.SetParent(parent, true);

            if ((SpawnKind)inst.spawnType == SpawnKind.ResourceGameObject)
                ConfigureStaticWorldResource(go);

            if (nobPrefab != null)
            {
                var nob = go.GetComponent<NetworkObject>();
                InstanceFinder.ServerManager.Spawn(nob);
            }

            ChunkedGameObjectStorage.Register(chunk, go);
        }

        private static void ConfigureStaticWorldResource(GameObject go)
        {
            if (go == null)
                return;

            var worldItem = go.GetComponent<WorldItemNetwork>();
            if (worldItem != null)
                worldItem.SetStaticWorldSpawn();
        }

        private static void SnapToGround(ref Vector3 pos, ref Quaternion rot)
        {
            Vector3 origin = pos + Vector3.up * 10f;

            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                Vector3.down,
                200f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (!IsGroundSnapCandidate(hit))
                    continue;

                pos = hit.point;
                Vector3 n = hit.normal.sqrMagnitude > 0.0001f ? hit.normal : Vector3.up;
                float yaw = rot.eulerAngles.y;
                Quaternion basis = Quaternion.FromToRotation(Vector3.up, n);
                rot = basis * Quaternion.Euler(0f, yaw, 0f);
                return;
            }
        }

        private static bool IsGroundSnapCandidate(RaycastHit hit)
        {
            if (hit.collider == null)
                return false;

            return hit.collider.GetComponentInParent<NetworkObject>() == null;
        }

    }
}
