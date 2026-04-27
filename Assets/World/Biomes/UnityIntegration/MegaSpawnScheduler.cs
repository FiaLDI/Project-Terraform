using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Biomes.Application;
using Biomes.Domain;

namespace Biomes.UnityIntegration {
    public class MegaSpawnScheduler : MonoBehaviour
    {
        public static MegaSpawnScheduler Instance;

        // сколько спавнов выполнять за кадр (можно регулировать)
        public int batchSize = 300;        // было 50 → очень мало
        public float maxTaskLifetime = 5f; // сек — страховка от зависших задач

        private class SpawnTask
        {
            public Vector2Int coord;
            public JobHandle job;
            public NativeList<SpawnInstance> spawnList;
            public NativeArray<Unity.Mathematics.float3> vertices;
            public GameObject chunkRoot;

            public float startTime;
            public int index;
            public bool completed;
        }

        private readonly List<SpawnTask> tasks = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void Schedule(Vector2Int coord, JobHandle job,
                             NativeList<SpawnInstance> spawnList,
                             NativeArray<Unity.Mathematics.float3> vertices,
                             GameObject chunkRoot)
        {
            tasks.Add(new SpawnTask
            {
                coord = coord,
                job = job,
                spawnList = spawnList,
                vertices = vertices,
                chunkRoot = chunkRoot,
                startTime = Time.realtimeSinceStartup
            });
        }

        private void Update()
        {
            float now = Time.realtimeSinceStartup;

            for (int t = tasks.Count - 1; t >= 0; t--)
            {
                var task = tasks[t];

                // 1) Чанк был выгружен → удаляем задачу
                if (task.chunkRoot == null)
                {
                    ForceDispose(task);
                    tasks.RemoveAt(t);
                    continue;
                }

                // 2) Job ещё выполняется
                if (!task.completed)
                {
                    if (task.job.IsCompleted)
                    {
                        task.job.Complete();
                        task.completed = true;

                        if (ChunkedInstanceLODSystem.Instance != null)
                            RegisterInstancedEnvironment(task);
                        else if (InstancedSpawnerSystem.Instance != null)
                            InstancedSpawnerSystem.Instance.AddSpawnInstances(task.spawnList);
                    }
                    else
                    {
                        // SAFETY: job висит слишком долго → форс-диспоз
                        if (now - task.startTime > maxTaskLifetime)
                        {
                            Debug.LogWarning($"[MegaSpawnScheduler] Job timeout → force dispose ({task.coord})");
                            ForceDispose(task);
                            tasks.RemoveAt(t);
                        }
                        continue;
                    }
                }

                // 3) Выполняем спавн порциями
                for (int i = 0; i < batchSize && task.index < task.spawnList.Length; i++)
                {
                    var inst = task.spawnList[task.index++];

                    if ((SpawnKind)inst.spawnType != SpawnKind.EnvironmentInstanced)
                    {
                        RuntimeSpawnerSystem.SpawnObject(
                            inst,
                            task.coord,
                            task.chunkRoot.transform
                        );
                    }
                }

                // 4) Если задача полностью выполнена → очищаем native память
                if (task.index >= task.spawnList.Length)
                {
                    ForceDispose(task);
                    tasks.RemoveAt(t);
                }
            }
        }

        private void ForceDispose(SpawnTask task)
        {
            if (!task.completed)
                task.job.Complete();

            if (task.spawnList.IsCreated)
                task.spawnList.Dispose();
            if (task.vertices.IsCreated)
                task.vertices.Dispose();
        }

        private static void RegisterInstancedEnvironment(SpawnTask task)
        {
            int count = task.spawnList.Length;

            for (int i = 0; i < count; i++)
            {
                var inst = task.spawnList[i];
                if ((SpawnKind)inst.spawnType != SpawnKind.EnvironmentInstanced)
                    continue;

                float random = inst.random01;
                if (random <= 0f)
                {
                    float value = Mathf.Sin(inst.position.x * 12.9898f + inst.position.z * 78.233f) * 43758.5453f;
                    random = value - Mathf.Floor(value);
                }

                bool alignToNormal = (inst.extraData & SpawnInstanceFlags.AlignToNormal) != 0;
                bool randomYRotation = (inst.extraData & SpawnInstanceFlags.RandomYRotation) != 0;
                float yRotationDegrees = randomYRotation ? random * 360f : 0f;
                Vector3 snappedPosition = new Vector3(inst.position.x, inst.position.y, inst.position.z);
                Vector3 snappedNormal = new Vector3(inst.normal.x, inst.normal.y, inst.normal.z);

                SnapInstancedToTerrain(ref snappedPosition, ref snappedNormal);

                ChunkedGameObjectStorage.RegisterInstanced(task.coord, inst.prefabIndex, new InstanceData
                {
                    position = snappedPosition,
                    normal = snappedNormal,
                    scale = inst.scale <= 0f ? 1f : inst.scale,
                    random = random,
                    yRotationDegrees = yRotationDegrees,
                    alignToNormal = alignToNormal,
                    biomeId = inst.biomeId
                });
            }
        }

        private static void SnapInstancedToTerrain(ref Vector3 position, ref Vector3 normal)
        {
            Vector3 origin = position + Vector3.up * 20f;

            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                Vector3.down,
                100f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            if (hits == null || hits.Length == 0)
                return;

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (!IsTerrainSnapCandidate(hit))
                    continue;

                position = hit.point;
                if (hit.normal.sqrMagnitude > 0.0001f)
                    normal = hit.normal.normalized;
                return;
            }
        }

        private static bool IsTerrainSnapCandidate(RaycastHit hit)
        {
            if (hit.collider == null)
                return false;

            if (hit.collider.GetComponentInParent<FishNet.Object.NetworkObject>() != null)
                return false;

            if (hit.collider is not MeshCollider)
                return false;

            if (hit.collider.gameObject.name == "Mesh_Collider_LOD0")
                return true;

            Transform parent = hit.collider.transform.parent;
            return parent != null && parent.name.StartsWith("Chunk_");
        }

        // ==== DEBUG API ====
        public int Debug_ActiveTaskCount => tasks.Count;

        private void OnDestroy()
        {
            for (int i = tasks.Count - 1; i >= 0; i--)
                ForceDispose(tasks[i]);

            tasks.Clear();

            if (Instance == this)
                Instance = null;
        }
    }   
}
