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

    private class SpawnTask
    {
        public Vector2Int coord;
        public JobHandle job;
        public NativeList<SpawnInstance> spawnList;
        public NativeArray<Unity.Mathematics.float3> vertices;
        public GameObject chunkRoot;
        public int index = 0;
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
            chunkRoot = chunkRoot
        });
    }

    private void Update()
    {
        for (int t = tasks.Count - 1; t >= 0; t--)
        {
            var task = tasks[t];

            if (!task.job.IsCompleted)
                continue;

            task.job.Complete();

            int batch = 50;

            for (int i = 0; i < batch && task.index < task.spawnList.Length; i++)
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

            if (task.index >= task.spawnList.Length)
            {
                task.spawnList.Dispose();
                task.vertices.Dispose();
                tasks.RemoveAt(t);
            }
        }
    }
}
