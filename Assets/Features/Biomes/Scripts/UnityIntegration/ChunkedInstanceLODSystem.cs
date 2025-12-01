using System.Collections.Generic;
using UnityEngine;

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
        [Tooltip("Обновлять чанки в несколько кадров (partial update)")]
        public int updatesPerFrame = 3;

        private int _chunkCursor = 0;

        private MaterialPropertyBlock _mpb;
        private Matrix4x4[] _matrices;
        private float[] _randoms;

        private readonly Dictionary<int, Mesh> _meshCache = new();
        private readonly Dictionary<int, Material[]> _matCache = new();

        private Camera _cam;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _mpb = new MaterialPropertyBlock();
            _matrices = new Matrix4x4[batchSize];
            _randoms  = new float[batchSize];
        }

        private void Start()
        {
            _cam = Camera.main;
        }

        // ================================
        // PUBLIC API — MAIN UPDATE LOGIC
        // ================================

        public void UpdateVisibleChunks(List<ChunkRuntimeData> activeChunks)
        {
            if (activeChunks == null || activeChunks.Count == 0)
                return;

            // камера может появиться ПОСЛЕ старта (например, в префабе игрока)
            if (_cam == null)
            {
                _cam = Camera.main;
                if (_cam == null)
                    return; // пока нет камеры — не рендерим LOD
            }

            int count = Mathf.Min(updatesPerFrame, activeChunks.Count);

            for (int i = 0; i < count; i++)
            {
                if (_chunkCursor >= activeChunks.Count)
                    _chunkCursor = 0;

                RenderChunk(activeChunks[_chunkCursor]);
                _chunkCursor++;
            }
        }

        // ================================
        // RENDER ONE CHUNK
        // ================================

        private void RenderChunk(ChunkRuntimeData chunk)
        {
            if (chunk == null || _cam == null)
                return;

            Vector3 camPos = _cam.transform.position;

            float lod0Sqr = lod0Distance * lod0Distance;
            float lod1Sqr = lod1Distance * lod1Distance;
            float lod2Sqr = lod2Distance * lod2Distance;

            foreach (var kv in chunk.Buckets)
            {
                int prefabIndex = kv.Key;
                var instances = kv.Value;

                if (!TryGetMesh(prefabIndex, out var mesh, out var mats))
                    continue;

                var material = mats != null && mats.Length > 0 ? mats[0] : null;
                if (material == null) continue;

                var lod0 = new List<(Matrix4x4, float)>();
                var lod1 = new List<(Matrix4x4, float)>();
                var lod2 = new List<(Matrix4x4, float)>();

                foreach (var inst in instances)
                {
                    float distance = (inst.position - camPos).sqrMagnitude;
                    if (distance > lod2Sqr)
                        continue;

                    Quaternion rot = inst.normal.sqrMagnitude > 0.0001f
                        ? Quaternion.LookRotation(inst.normal)
                        : Quaternion.identity;

                    Matrix4x4 m = Matrix4x4.TRS(
                        inst.position,
                        rot,
                        Vector3.one * inst.scale
                    );

                    if (distance <= lod0Sqr)      lod0.Add((m, inst.random));
                    else if (distance <= lod1Sqr) lod1.Add((m, inst.random));
                    else                          lod2.Add((m, inst.random));
                }

                DrawBatch(mesh, material, lod0);
                DrawBatch(mesh, material, lod1);
                DrawBatch(mesh, material, lod2);
            }
        }

        // ================================
        // DRAW BATCH
        // ================================

        private void DrawBatch(Mesh mesh, Material mat, List<(Matrix4x4, float)> list)
        {
            int total = list.Count;
            if (total == 0) return;

            int start = 0;
            while (start < total)
            {
                int count = Mathf.Min(batchSize, total - start);

                for (int i = 0; i < count; i++)
                {
                    _matrices[i] = list[start + i].Item1;
                    _randoms[i]  = list[start + i].Item2;
                }

                _mpb.Clear();
                _mpb.SetFloatArray("_InstanceRandom", _randoms);

                Graphics.DrawMeshInstanced(mesh, 0, mat, _matrices, count, _mpb);

                start += count;
            }
        }

        // ================================
        // MESH / MATERIAL CACHE
        // ================================

        private bool TryGetMesh(int id, out Mesh mesh, out Material[] mats)
        {
            if (_meshCache.TryGetValue(id, out mesh) &&
                _matCache.TryGetValue(id, out mats))
                return true;

            if (InstanceRegistry.TryGetInstancedMesh(id, out mesh, out mats))
            {
                _meshCache[id] = mesh;
                _matCache[id]  = mats;
                return true;
            }

            return false;
        }
    }
}
