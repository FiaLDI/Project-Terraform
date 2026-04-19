using System.Collections.Generic;
using UnityEngine;
using Features.Camera.UnityIntegration;
using Biomes.Application;

namespace Biomes.UnityIntegration
{
    public class ChunkedInstanceLODSystem : MonoBehaviour
    {
        public static ChunkedInstanceLODSystem Instance;

        [Header("LOD Distances")]
        public float lod0Distance = 50f;
        public float lod1Distance = 120f;
        public float lod2Distance = 250f;

        [Header("Batch Settings")]
        public int batchSize = 1023;

        [Header("Update Settings")]
        [Tooltip("Reserved for future optimization. Instanced chunks must render every frame.")]
        public int updatesPerFrame = 3;

        private MaterialPropertyBlock _mpb;
        private Matrix4x4[] _matrices;
        private float[] _randoms;

        private readonly Dictionary<int, Mesh> _meshCache = new();
        private readonly Dictionary<int, Material[]> _matCache = new();
        private readonly Dictionary<int, Matrix4x4> _localMatrixCache = new();
        private readonly Dictionary<int, Vector3> _rootScaleCache = new();
        private readonly Dictionary<int, float> _groundOffsetCache = new();
        private readonly Dictionary<int, Material> _ownedInstancedMaterials = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _mpb = new MaterialPropertyBlock();
            _matrices = new Matrix4x4[batchSize];
            _randoms = new float[batchSize];
        }

        private void OnDestroy()
        {
            foreach (var kv in _ownedInstancedMaterials)
            {
                if (kv.Value != null)
                    Destroy(kv.Value);
            }

            _ownedInstancedMaterials.Clear();

            if (Instance == this)
                Instance = null;
        }

        public void UpdateVisibleChunks(List<ChunkRuntimeData> activeChunks)
        {
            if (activeChunks == null || activeChunks.Count == 0)
                return;

            var cam = GetActiveCamera();
            if (cam == null)
                return;

            for (int i = 0; i < activeChunks.Count; i++)
            {
                RenderChunk(activeChunks[i], cam);
            }
        }

        private void RenderChunk(ChunkRuntimeData chunk, Camera cam)
        {
            if (chunk == null || cam == null)
                return;

            Vector3 camPos = cam.transform.position;

            float lod0Sqr = lod0Distance * lod0Distance;
            float lod1Sqr = lod1Distance * lod1Distance;
            float lod2Sqr = lod2Distance * lod2Distance;

            foreach (var kv in chunk.Buckets)
            {
                int prefabIndex = kv.Key;
                var instances = kv.Value;

                if (!TryGetRenderData(
                        prefabIndex,
                        out Mesh mesh,
                        out Material[] mats,
                        out Matrix4x4 localMatrix,
                        out Vector3 rootScale,
                        out float groundOffset))
                    continue;

                var mat = (mats != null && mats.Length > 0) ? mats[0] : null;
                if (mat == null)
                    continue;

                var lod0 = new List<(Matrix4x4, float)>();
                var lod1 = new List<(Matrix4x4, float)>();
                var lod2 = new List<(Matrix4x4, float)>();

                foreach (var inst in instances)
                {
                    float d2 = (inst.position - camPos).sqrMagnitude;
                    if (d2 > lod2Sqr)
                        continue;

                    Quaternion rot = inst.normal.sqrMagnitude > 0.0001f
                        ? Quaternion.FromToRotation(Vector3.up, inst.normal)
                        : Quaternion.identity;

                    Vector3 finalScale = Vector3.Scale(rootScale, Vector3.one * inst.scale);
                    Vector3 renderPos = inst.position + (rot * Vector3.up) * (-groundOffset * finalScale.y);

                    Matrix4x4 rootMatrix = Matrix4x4.TRS(
                        renderPos,
                        rot,
                        finalScale
                    );
                    Matrix4x4 m = rootMatrix * localMatrix;

                    if (d2 <= lod0Sqr) lod0.Add((m, inst.random));
                    else if (d2 <= lod1Sqr) lod1.Add((m, inst.random));
                    else lod2.Add((m, inst.random));
                }

                DrawBatch(mesh, mat, lod0);
                DrawBatch(mesh, mat, lod1);
                DrawBatch(mesh, mat, lod2);
            }
        }

        private void DrawBatch(Mesh mesh, Material mat, List<(Matrix4x4, float)> list)
        {
            int total = list.Count;
            if (total == 0)
                return;

            int index = 0;
            while (index < total)
            {
                int count = Mathf.Min(batchSize, total - index);

                for (int i = 0; i < count; i++)
                {
                    _matrices[i] = list[index + i].Item1;
                    _randoms[i] = list[index + i].Item2;
                }

                _mpb.Clear();
                _mpb.SetFloatArray("_InstanceRandom", _randoms);

                Graphics.DrawMeshInstanced(mesh, 0, mat, _matrices, count, _mpb);

                index += count;
            }
        }

        private Camera GetActiveCamera()
        {
            if (CameraRegistry.Instance != null &&
                CameraRegistry.Instance.CurrentCamera != null)
                return CameraRegistry.Instance.CurrentCamera;

            return null;
        }

        private bool TryGetRenderData(
            int id,
            out Mesh mesh,
            out Material[] mats,
            out Matrix4x4 localMatrix,
            out Vector3 rootScale,
            out float groundOffset)
        {
            if (_meshCache.TryGetValue(id, out mesh) &&
                _matCache.TryGetValue(id, out mats) &&
                _localMatrixCache.TryGetValue(id, out localMatrix) &&
                _rootScaleCache.TryGetValue(id, out rootScale) &&
                _groundOffsetCache.TryGetValue(id, out groundOffset))
                return true;

            if (InstanceRegistry.TryGetInstancedRenderData(
                    id,
                    out mesh,
                    out mats,
                    out localMatrix,
                    out rootScale,
                    out groundOffset))
            {
                var mat = (mats != null && mats.Length > 0) ? mats[0] : null;
                var ensured = EnsureInstancedMaterial(id, mat);
                mats = ensured != null ? new[] { ensured } : mats;

                _meshCache[id] = mesh;
                _matCache[id] = mats;
                _localMatrixCache[id] = localMatrix;
                _rootScaleCache[id] = rootScale;
                _groundOffsetCache[id] = groundOffset;
                return true;
            }

            localMatrix = Matrix4x4.identity;
            rootScale = Vector3.one;
            groundOffset = 0f;
            return false;
        }

        private Material EnsureInstancedMaterial(int prefabId, Material source)
        {
            if (source == null)
                return null;

            if (source.enableInstancing)
                return source;

            if (_ownedInstancedMaterials.TryGetValue(prefabId, out var existing) && existing != null)
                return existing;

            var instanced = new Material(source)
            {
                name = source.name + " (Instanced)"
            };
            instanced.enableInstancing = true;
            _ownedInstancedMaterials[prefabId] = instanced;

            Debug.LogWarning(
                $"[ChunkedInstanceLOD] Material '{source.name}' had instancing disabled. " +
                $"Created instanced copy '{instanced.name}'.");

            return instanced;
        }

        public void ClearRuntimeState()
        {
            foreach (var kv in _ownedInstancedMaterials)
            {
                if (kv.Value != null)
                    Destroy(kv.Value);
            }

            _ownedInstancedMaterials.Clear();
            _meshCache.Clear();
            _matCache.Clear();
            _localMatrixCache.Clear();
            _rootScaleCache.Clear();
            _groundOffsetCache.Clear();
        }
    }
}
