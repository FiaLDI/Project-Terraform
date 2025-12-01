using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Features.Biomes.Utility
{
    [BurstCompile]
    public struct BurstRecalculateNormalsJob : IJob
    {
        [ReadOnly] public NativeArray<float3> vertices;
        [ReadOnly] public NativeArray<int> triangles;

        // результат
        public NativeArray<float3> normals;

        public void Execute()
        {
            // 1) обнуляем нормали
            for (int i = 0; i < normals.Length; i++)
                normals[i] = float3.zero;

            // 2) аккумулируем нормали по треугольникам
            int tCount = triangles.Length;
            for (int t = 0; t < tCount; t += 3)
            {
                int i0 = triangles[t];
                int i1 = triangles[t + 1];
                int i2 = triangles[t + 2];

                float3 v0 = vertices[i0];
                float3 v1 = vertices[i1];
                float3 v2 = vertices[i2];

                float3 e1 = v1 - v0;
                float3 e2 = v2 - v0;

                float3 fn = math.cross(e1, e2); // не нормализуем пока

                normals[i0] += fn;
                normals[i1] += fn;
                normals[i2] += fn;
            }

            // 3) нормализуем
            for (int i = 0; i < normals.Length; i++)
            {
                float3 n = normals[i];
                float lenSq = math.lengthsq(n);
                if (lenSq < 1e-6f)
                {
                    normals[i] = new float3(0, 1, 0);
                }
                else
                {
                    normals[i] = math.normalize(n);
                }
            }
        }
    }

    public static class BurstMeshUtility
    {
        /// <summary>
        /// Пересчёт нормалей меша через Burst-джоб.
        /// </summary>
        public static void RecalculateNormalsBurst(Mesh mesh)
        {
            if (mesh == null) return;

            Vector3[] vManaged = mesh.vertices;
            int[] tManaged = mesh.triangles;

            int vCount = vManaged.Length;
            int tCount = tManaged.Length;

            if (vCount == 0 || tCount == 0)
                return;

            var vertices = new NativeArray<float3>(vCount, Allocator.TempJob);
            var triangles = new NativeArray<int>(tCount, Allocator.TempJob);
            var normals = new NativeArray<float3>(vCount, Allocator.TempJob);

            for (int i = 0; i < vCount; i++)
            {
                Vector3 v = vManaged[i];
                vertices[i] = new float3(v.x, v.y, v.z);
            }

            for (int i = 0; i < tCount; i++)
                triangles[i] = tManaged[i];

            var job = new BurstRecalculateNormalsJob
            {
                vertices = vertices,
                triangles = triangles,
                normals = normals
            };

            JobHandle handle = job.Schedule();
            handle.Complete();

            Vector3[] nManaged = new Vector3[vCount];
            for (int i = 0; i < vCount; i++)
            {
                float3 n = normals[i];
                nManaged[i] = new Vector3(n.x, n.y, n.z);
            }

            mesh.normals = nManaged;

            vertices.Dispose();
            triangles.Dispose();
            normals.Dispose();
        }
    }
}
