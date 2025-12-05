using UnityEngine;
using Unity.Mathematics;
using Features.Pooling;
using Features.Biomes.Domain;
using Features.Biomes.Application.Spawning;
using Features.Biomes.Application;

namespace Features.Biomes.UnityIntegration
{
    /// <summary>
    /// Спавн обычных GameObject (ресурсы, враги, квесты) по данным SpawnInstance.
    /// Работает через SmartPool + ChunkedGameObjectStorage.
    /// </summary>
    public static class RuntimeSpawnerSystem
    {
        private static bool IsFinite(float v) =>
            !float.IsNaN(v) && !float.IsInfinity(v);

        private static bool IsFinite(float3 v) =>
            IsFinite(v.x) && IsFinite(v.y) && IsFinite(v.z);

        /// <summary>
        /// Основной метод спавна. Обязателен parent — Transform чанка.
        /// </summary>
        public static void SpawnObject(SpawnInstance inst, Vector2Int chunk, Transform parent)
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

            // 🔗 ВАЖНО: назначаем родителя чанка
            if (parent != null)
                pooled.transform.SetParent(parent, true); // worldPositionStays = true

            // ========== ПОЗИЦИЯ / РОТАЦИЯ ==========
            Vector3 pos = new Vector3(inst.position.x, inst.position.y, inst.position.z);
            Vector3 normalVec = new Vector3(inst.normal.x, inst.normal.y, inst.normal.z);

            Quaternion rotation =
                normalVec.sqrMagnitude > 0.0001f
                ? Quaternion.FromToRotation(Vector3.up, normalVec)
                : Quaternion.identity;

            pooled.transform.position   = pos;
            pooled.transform.rotation   = rotation;
            pooled.transform.localScale = Vector3.one * inst.scale;

            SnapToGroundIgnoringSelf(pooled.transform, ref pos, ref rotation);
            pooled.transform.SetPositionAndRotation(pos, rotation);


            // ========== РЕГИСТРАЦИЯ В ХРАНИЛИЩЕ ==========
            ChunkedGameObjectStorage.Register(chunk, pooled.gameObject);
        }

        /// <summary>
        /// Оверлоад для старого кода (без parent).
        /// Если не передали родителя — объект окажется в корне сцены.
        /// Лучше не использовать в новых местах.
        /// </summary>
        public static void SpawnObject(SpawnInstance inst, Vector2Int chunk)
        {
            SpawnObject(inst, chunk, null);
        }

        private static void SnapToGroundIgnoringSelf(Transform tr, ref Vector3 pos, ref Quaternion rot)
        {
            // запомним слой, потом вернём
            int originalLayer = tr.gameObject.layer;

            // временно переведём объект в IgnoreRaycast (2), чтобы луч не попадал по нему
            tr.gameObject.layer = 2; // "Ignore Raycast" — стандартный unity-слой

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

                // нормаль поверхности
                Vector3 n = hit.normal.sqrMagnitude > 0.0001f ? hit.normal : Vector3.up;

                // хотим сохранить текущий yaw (поворот по Y), но выровнять по нормали
                float yaw = rot.eulerAngles.y;
                Quaternion basis = Quaternion.FromToRotation(Vector3.up, n);
                rot = basis * Quaternion.Euler(0f, yaw, 0f);
            }

            // вернули слой назад
            tr.gameObject.layer = originalLayer;
        }

    }
}
