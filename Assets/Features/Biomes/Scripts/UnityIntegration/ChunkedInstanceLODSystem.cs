using System.Collections.Generic;
using UnityEngine;
using Features.Camera.UnityIntegration;

namespace Features.Biomes.Application
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
        [Tooltip("Обновлять чанки не за 1 кадр, а порциями")]
        public int updatesPerFrame = 3;

        private int _chunkCursor = 0;

        private MaterialPropertyBlock _mpb;
        private Matrix4x4[] _matrices;
        private float[] _randoms;

        private readonly Dictionary<int, Mesh> _meshCache = new();
        private readonly Dictionary<int, Material[]> _matCache = new();

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _mpb = new MaterialPropertyBlock();
            _matrices = new Matrix4x4[batchSize];
            _randoms = new float[batchSize];
        }

        // ====================================================================
        // MAIN UPDATE ENTRY
        // ====================================================================

        public void UpdateVisibleChunks(List<ChunkRuntimeData> activeChunks)
        {
            if (activeChunks == null || activeChunks.Count == 0)
                return;

            // Берём текущую камеру через CameraRegistry
            var cam = GetActiveCamera();
            if (cam == null)
                return; // камера ещё не появилась → пропускаем кадр

            int iterations = Mathf.Min(updatesPerFrame, activeChunks.Count);

            for (int i = 0; i < iterations; i++)
            {
                if (_chunkCursor >= activeChunks.Count)
                    _chunkCursor = 0;

                RenderChunk(activeChunks[_chunkCursor], cam);
                _chunkCursor++;
            }
        }

        // ====================================================================
        // RENDER SINGLE CHUNK
        // ====================================================================
        private void RenderChunk(ChunkRuntimeData chunk, UnityEngine.Camera cam)
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

                if (!TryGetMesh(prefabIndex, out Mesh mesh, out Material[] mats))
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

                    Matrix4x4 m = Matrix4x4.TRS(
                        inst.position,
                        rot,
                        Vector3.one * inst.scale
                    );

                    if (d2 <= lod0Sqr) lod0.Add((m, inst.random));
                    else if (d2 <= lod1Sqr) lod1.Add((m, inst.random));
                    else lod2.Add((m, inst.random));
                }

                DrawBatch(mesh, mat, lod0);
                DrawBatch(mesh, mat, lod1);
                DrawBatch(mesh, mat, lod2);
            }
        }

        // ====================================================================
        // BATCH DRAW
        // ====================================================================
        private void DrawBatch(Mesh mesh, Material mat, List<(Matrix4x4, float)> list)
        {
            int total = list.Count;
            if (total == 0) return;

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

        // ====================================================================
        // CAMERA RESOLUTION
        // ====================================================================
        private UnityEngine.Camera GetActiveCamera()
        {
            // 1) активная камера из CameraRegistry
            if (CameraRegistry.Instance != null &&
                CameraRegistry.Instance.CurrentCamera != null)
                return CameraRegistry.Instance.CurrentCamera;

            // 2) камеры ещё нет → ничего не рендерим
            return null;
        }

        // ====================================================================
        // CACHE LOOKUP
        // ====================================================================
        private bool TryGetMesh(int id, out Mesh mesh, out Material[] mats)
        {
            if (_meshCache.TryGetValue(id, out mesh) &&
                _matCache.TryGetValue(id, out mats))
                return true;

            if (InstanceRegistry.TryGetInstancedMesh(id, out mesh, out mats))
            {
                _meshCache[id] = mesh;
                _matCache[id] = mats;
                return true;
            }

            return false;
        }
    }
}
