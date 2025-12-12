using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Features.Biomes.Domain;
using Features.Biomes.UnityIntegration;
using Features.Biomes.Application.Spawning;

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
        if (task.spawnList.IsCreated)
            task.spawnList.Dispose();
        if (task.vertices.IsCreated)
            task.vertices.Dispose();
    }

    // ==== DEBUG API ====
    public int Debug_ActiveTaskCount => tasks.Count;
}
