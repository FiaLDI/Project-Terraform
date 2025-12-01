using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Features.Biomes.Domain;
using Features.Biomes.UnityIntegration;

namespace Features.Biomes.Application
{
    public class InstancedSpawnerSystem : MonoBehaviour
    {
        public static InstancedSpawnerSystem Instance { get; private set; }

        [Header("LOD Distances")]
        public float lod0Distance = 50f;
        public float lod1Distance = 120f;
        public float lod2Distance = 250f;

        [Header("Batch Settings")]
        public int batchSize = 1023;

        [Header("Target")]
        public Transform targetOverride;

        private class InstanceData
        {
            public Vector3 position;
            public Vector3 normal;
            public float   scale;
            public float   random;
            public int     biomeId;
        }

        private class InstanceBucket
        {
            public Mesh     Mesh;
            public Material Material;

            public readonly List<InstanceData> Instances = new();

            public readonly List<Matrix4x4> MatricesLod0 = new();
            public readonly List<Matrix4x4> MatricesLod1 = new();
            public readonly List<Matrix4x4> MatricesLod2 = new();

            public readonly List<float> RandomLod0 = new();
            public readonly List<float> RandomLod1 = new();
            public readonly List<float> RandomLod2 = new();
        }

        private readonly Dictionary<int, InstanceBucket> _buckets = new();

        private Camera _mainCam;
        private MaterialPropertyBlock _mpb;
        private Matrix4x4[] _matrixBatch;
        private float[] _randomBatch;

        private const int MaxBatchHardLimit = 1023;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            int cap = Mathf.Clamp(batchSize, 1, MaxBatchHardLimit);
            _matrixBatch = new Matrix4x4[cap];
            _randomBatch = new float[cap];
            _mpb = new MaterialPropertyBlock();
        }

        private void Start()
        {
            _mainCam = Camera.main;
        }

        public void Init(Transform target)
        {
            targetOverride = target;
        }

        /// <summary>
        /// Принимает список SpawnInstance из MegaSpawnJob.
        /// Обрабатывает ТОЛЬКО EnvironmentInstanced.
        /// Все ресурсы/враги/квесты спавнятся через RuntimeSpawnerSystem из Chunk.
        /// </summary>
        public void AddSpawnInstances(NativeList<SpawnInstance> spawnList)
        {
            int count = spawnList.Length;

            for (int i = 0; i < count; i++)
            {
                var inst = spawnList[i];

                if (inst.spawnType != (int)SpawnKind.EnvironmentInstanced)
                    continue;

                if (!InstanceRegistry.TryGetInstancedMesh(inst.prefabIndex, out var mesh, out var mats))
                    continue;

                var mat = (mats != null && mats.Length > 0) ? mats[0] : null;
                if (mesh == null || mat == null)
                    continue;

                if (!_buckets.TryGetValue(inst.prefabIndex, out var bucket))
                {
                    bucket = new InstanceBucket
                    {
                        Mesh     = mesh,
                        Material = EnsureInstancedMaterial(mat)
                    };
                    _buckets[inst.prefabIndex] = bucket;
                }


                float3 p = inst.position;
                float random = math.frac(math.sin(p.x * 12.9898f + p.z * 78.233f) * 43758.5453f);

                bucket.Instances.Add(new InstanceData
                {
                    position = new Vector3(p.x, p.y, p.z),
                    normal   = new Vector3(inst.normal.x, inst.normal.y, inst.normal.z),
                    scale    = inst.scale <= 0f ? 1f : inst.scale,
                    random   = random,
                    biomeId  = inst.biomeId
                });
            }
        }

        private void LateUpdate()
        {
            if (_buckets.Count == 0)
                return;

            Vector3 camPos = GetTargetPosition();
            if (!IsFinite(camPos))
                return;

            float lod0Sqr = lod0Distance * lod0Distance;
            float lod1Sqr = lod1Distance * lod1Distance;
            float lod2Sqr = lod2Distance * lod2Distance;

            foreach (var kv in _buckets)
            {
                var bucket = kv.Value;
                if (bucket.Mesh == null || bucket.Material == null)
                    continue;
                
                if (!bucket.Material.enableInstancing)
                {
                    Debug.LogError($"[InstancedSpawner] Material '{bucket.Material.name}' still has instancing disabled. Skipping prefabIndex={kv.Key}");
                    continue;
                }

                bucket.MatricesLod0.Clear();
                bucket.MatricesLod1.Clear();
                bucket.MatricesLod2.Clear();
                bucket.RandomLod0.Clear();
                bucket.RandomLod1.Clear();
                bucket.RandomLod2.Clear();

                foreach (var inst in bucket.Instances)
                {
                    if (!IsFinite(inst.position))
                        continue;

                    float d2 = (inst.position - camPos).sqrMagnitude;
                    if (d2 > lod2Sqr)
                        continue;

                    if (!TryBuildMatrix(inst.position, inst.normal, inst.scale, out Matrix4x4 m))
                        continue;

                    if (d2 <= lod0Sqr)
                    {
                        bucket.MatricesLod0.Add(m);
                        bucket.RandomLod0.Add(inst.random);
                    }
                    else if (d2 <= lod1Sqr)
                    {
                        bucket.MatricesLod1.Add(m);
                        bucket.RandomLod1.Add(inst.random);
                    }
                    else
                    {
                        bucket.MatricesLod2.Add(m);
                        bucket.RandomLod2.Add(inst.random);
                    }
                }

                RenderBucket(bucket, bucket.MatricesLod0, bucket.RandomLod0);
                RenderBucket(bucket, bucket.MatricesLod1, bucket.RandomLod1);
                RenderBucket(bucket, bucket.MatricesLod2, bucket.RandomLod2);
            }
        }

        private Vector3 GetTargetPosition()
        {
            if (targetOverride != null)
                return targetOverride.position;

            if (RuntimeWorldGenerator.PlayerInstance != null)
                return RuntimeWorldGenerator.PlayerInstance.transform.position;

            if (_mainCam != null)
                return _mainCam.transform.position;

            if (Camera.main != null)
            {
                _mainCam = Camera.main;
                return _mainCam.transform.position;
            }

            return Vector3.zero;
        }

        private static bool IsFinite(Vector3 v)
        {
            return !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
                     float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
        }

        private static bool IsMatrixFinite(Matrix4x4 m)
        {
            for (int i = 0; i < 16; i++)
            {
                if (float.IsNaN(m[i]) || float.IsInfinity(m[i]))
                    return false;
            }
            return true;
        }

        private static bool TryBuildMatrix(Vector3 pos, Vector3 normal, float scale, out Matrix4x4 m)
        {
            if (!IsFinite(pos))
            {
                m = Matrix4x4.identity;
                return false;
            }

            if (!IsFinite(normal) || normal.sqrMagnitude < 0.0001f)
                normal = Vector3.up;

            normal.Normalize();
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, normal);

            if (float.IsNaN(rot.x) || float.IsNaN(rot.y) || float.IsNaN(rot.z) || float.IsNaN(rot.w) ||
                float.IsInfinity(rot.x) || float.IsInfinity(rot.y) || float.IsInfinity(rot.z) || float.IsInfinity(rot.w))
            {
                rot = Quaternion.identity;
            }

            if (scale <= 0f || float.IsNaN(scale) || float.IsInfinity(scale))
                scale = 1f;

            m = Matrix4x4.TRS(pos, rot, Vector3.one * scale);
            return IsMatrixFinite(m);
        }

        private void RenderBucket(InstanceBucket bucket, List<Matrix4x4> matrices, List<float> randoms)
        {
            int total = matrices.Count;
            if (total == 0)
                return;

            int cap = Mathf.Min(batchSize, MaxBatchHardLimit);
            int index = 0;

            while (index < total)
            {
                int count = Mathf.Min(cap, total - index);

                for (int i = 0; i < count; i++)
                {
                    _matrixBatch[i] = matrices[index + i];
                    _randomBatch[i] = randoms[index + i];
                }

                _mpb.Clear();
                _mpb.SetFloatArray("_InstanceRandom", _randomBatch);

                Graphics.DrawMeshInstanced(
                    bucket.Mesh,
                    0,
                    bucket.Material,
                    _matrixBatch,
                    count,
                    _mpb,
                    UnityEngine.Rendering.ShadowCastingMode.On,
                    true
                );

                index += count;
            }
        }

        private static Material EnsureInstancedMaterial(Material source)
        {
            if (source == null)
                return null;

            if (source.enableInstancing)
                return source;

            var instanced = new Material(source)
            {
                name = source.name + " (Instanced)"
            };
            instanced.enableInstancing = true;

            Debug.LogWarning($"[InstancedSpawner] Material '{source.name}' had instancing disabled. Created instanced copy '{instanced.name}'.");

            return instanced;
        }
    }
}
