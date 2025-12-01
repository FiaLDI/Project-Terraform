using UnityEngine;
using Unity.Mathematics;
using Features.Pooling;
using Features.Biomes.Domain;
using Features.Biomes.Application;
using Features.Biomes.Application.Spawning;

namespace Features.Biomes.UnityIntegration
{
    public static class RuntimeSpawnerSystem
    {
        private static bool IsFinite(float v) =>
            !float.IsNaN(v) && !float.IsInfinity(v);

        private static bool IsFinite(float3 v) =>
            IsFinite(v.x) && IsFinite(v.y) && IsFinite(v.z);

        public static void SpawnObject(SpawnInstance inst, Vector2Int chunk)
        {
            // ========== ЗАЩИТА ==========

            if (!IsFinite(inst.position))
            {
                Debug.LogError(
                    $"[RuntimeSpawner] NaN position for prefabIndex={inst.prefabIndex}, " +
                    $"spawnType={(SpawnKind)inst.spawnType}, chunk={chunk}, pos={inst.position}");
                return;
            }

            if (!IsFinite(inst.normal) || math.lengthsq(inst.normal) < 0.0001f)
                inst.normal = new float3(0, 1, 0);

            if (!IsFinite(inst.scale) || inst.scale <= 0f)
                inst.scale = 1f;

            // ========== ПОЛЛИНГ ==========

            if (!InstanceRegistry.TryGetPrefab(inst.prefabIndex, out var prefab))
                return;

            var pooled = SmartPool.Instance.Get(inst.prefabIndex, prefab);

            if (pooled.meta == null)
                pooled.meta = pooled.gameObject.AddComponent<PoolMeta>();

            pooled.meta.prefabIndex = inst.prefabIndex;

            // ========== ПОЗИЦИЯ И РОТАЦИЯ (уже идеальные из MegaSpawnJob) ==========

            Vector3 pos = new Vector3(inst.position.x, inst.position.y, inst.position.z);
            Vector3 normalVec = new Vector3(inst.normal.x, inst.normal.y, inst.normal.z);

            Quaternion rotation =
                normalVec.sqrMagnitude > 0.0001f
                ? Quaternion.FromToRotation(Vector3.up, normalVec)
                : Quaternion.identity;

            // ========== ПРИМЕНЕНИЕ ==========
            pooled.transform.position   = pos;
            pooled.transform.rotation   = rotation;
            pooled.transform.localScale = Vector3.one * inst.scale;

            // ========== РЕГИСТРАЦИЯ ==========
            ChunkedGameObjectStorage.Register(chunk, pooled.gameObject);
        }
    }
}
