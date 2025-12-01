using UnityEngine;
using Unity.Mathematics;
using Features.Biomes.Domain;

namespace Features.Biomes.UnityIntegration
{
    public static class TerrainMeshGenerator
    {
        // =====================================================================
        // MAIN ENTRY
        // =====================================================================
        public static Mesh GenerateMeshSync(
            Vector2Int coord,
            int chunkSize,
            int resolution,
            WorldConfig world,
            bool useLowPoly)
        {
            // 1) Высоты LOD0
            float[,] heightmapLOD0 = GenerateHeightmapLOD0(coord, chunkSize, world);

            // 2) Downsample → LODn
            float[,] heightmap =
                (resolution == chunkSize)
                    ? heightmapLOD0
                    : Downsample(heightmapLOD0, chunkSize, resolution);

            // 3) Получаем доминирующий биом → его UV tiling
            var blend = world.GetDominantBiome(coord);
            float tiling = (blend.biome != null ? blend.biome.textureTiling : 1f);

            // 4) Генерация меша
            return BuildMeshFromHeightmap(heightmap, resolution, chunkSize, useLowPoly, tiling);
        }

        // =====================================================================
        // LOD0 (полная карта высот)
        // =====================================================================
        private static float[,] GenerateHeightmapLOD0(
            Vector2Int coord,
            int chunkSize,
            WorldConfig world)
        {
            int res = chunkSize;
            int size = res + 1;
            float[,] hmap = new float[size, size];

            float chunkX = coord.x * chunkSize;
            float chunkZ = coord.y * chunkSize;

            float step = (float)chunkSize / res;

            for (int z = 0; z <= res; z++)
            {
                for (int x = 0; x <= res; x++)
                {
                    float wx = chunkX + x * step;
                    float wz = chunkZ + z * step;

                    float h = SampleHeightBlended(world, wx, wz);
                    if (!float.IsFinite(h)) h = 0f;

                    hmap[z, x] = h;
                }
            }
            return hmap;
        }

        // вместо старого SampleHeightBlended
        private static float SampleHeightBlended(WorldConfig world, float wx, float wz)
        {
            var blends = world.GetBiomeBlend(new Vector3(wx, 0, wz));
            if (blends == null || blends.Length == 0)
                return 0f;

            // Находим главный биом (у которого локальный вес выше остальных)
            BiomeConfig main = blends[0].biome;
            float best = blends[0].weight;

            for (int i = 1; i < blends.Length; i++)
            {
                if (blends[i].weight > best)
                {
                    best = blends[i].weight;
                    main = blends[i].biome;
                }
            }

            // Высота доминирующего биома
            float hMain = BiomeHeightUtility.GetHeight(main, wx, wz);

            // Если он не хочет смешиваться → возврат
            if (main.blendStrength <= 0.001f)
                return hMain;

            // Смешиваем с соседними биомами в зависимости от их веса
            float sumH = hMain * best;
            float sumW = best;

            foreach (var b in blends)
            {
                if (b.biome == null || b.biome == main)
                    continue;

                float w = b.weight * main.blendStrength; 
                sumH += BiomeHeightUtility.GetHeight(b.biome, wx, wz) * w;
                sumW += w;
            }

            return sumW > 0f ? sumH / sumW : hMain;
        }



        // =====================================================================
        // 2) DOWNSAMPLE
        // =====================================================================
        private static float[,] Downsample(float[,] src, int srcRes, int dstRes)
        {
            int srcSize = srcRes + 1;
            int dstSize = dstRes + 1;
            float factor = (float)srcRes / dstRes;

            float[,] hmap = new float[dstSize, dstSize];

            for (int z = 0; z < dstSize; z++)
            {
                for (int x = 0; x < dstSize; x++)
                {
                    float sx = x * factor;
                    float sz = z * factor;

                    int x0 = Mathf.FloorToInt(sx);
                    int z0 = Mathf.FloorToInt(sz);
                    int x1 = Mathf.Min(x0 + 1, srcSize - 1);
                    int z1 = Mathf.Min(z0 + 1, srcSize - 1);

                    float tx = sx - x0;
                    float tz = sz - z0;

                    float h00 = src[z0, x0];
                    float h10 = src[z0, x1];
                    float h01 = src[z1, x0];
                    float h11 = src[z1, x1];

                    float hx0 = Mathf.Lerp(h00, h10, tx);
                    float hx1 = Mathf.Lerp(h01, h11, tx);

                    hmap[z, x] = Mathf.Lerp(hx0, hx1, tz);
                }
            }

            return hmap;
        }

        // =====================================================================
        // 3) Build mesh (smooth или flat)
        // =====================================================================
        private static Mesh BuildMeshFromHeightmap(
            float[,] hmap,
            int resolution,
            int chunkSize,
            bool useLowPoly,
            float tiling)
        {
            return useLowPoly
                ? BuildLowPolyMesh(hmap, resolution, chunkSize, tiling)
                : BuildSmoothMesh(hmap, resolution, chunkSize, tiling);
        }

        // =====================================================================
        // SMOOTH MESH
        // =====================================================================
        private static Mesh BuildSmoothMesh(float[,] hmap, int resolution, int chunkSize, float tiling)
        {
            int size = resolution + 1;

            Vector3[] verts = new Vector3[size * size];
            Vector2[] uvs   = new Vector2[size * size];
            int[] tris      = new int[resolution * resolution * 6];

            float step = (float)chunkSize / resolution;

            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    float h = hmap[z, x];

                    int idx = z * size + x;
                    verts[idx] = new Vector3(x * step, h, z * step);

                    uvs[idx] = new Vector2(
                        (float)x / resolution * tiling,
                        (float)z / resolution * tiling
                    );
                }
            }

            int ti = 0;
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int v = z * size + x;

                    tris[ti++] = v;
                    tris[ti++] = v + size;
                    tris[ti++] = v + 1;

                    tris[ti++] = v + 1;
                    tris[ti++] = v + size;
                    tris[ti++] = v + size + 1;
                }
            }

            Mesh m = new Mesh();
            m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            m.vertices  = verts;
            m.uv        = uvs;
            m.triangles = tris;

            m.RecalculateNormals();
            m.RecalculateBounds();

            return m;
        }

        // =====================================================================
        // LOW POLY (flat)
        // =====================================================================
        private static Mesh BuildLowPolyMesh(float[,] hmap, int resolution, int chunkSize, float tiling)
        {
            int quadCount = resolution * resolution;
            int vertCount = quadCount * 6;

            Vector3[] verts   = new Vector3[vertCount];
            Vector3[] normals = new Vector3[vertCount];
            Vector2[] uvs     = new Vector2[vertCount];
            int[]     tris    = new int[vertCount];

            float step = (float)chunkSize / resolution;

            int vi = 0;

            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    Vector3 v00 = new Vector3(x * step,             hmap[z,     x],     z * step);
                    Vector3 v10 = new Vector3((x + 1) * step,       hmap[z,     x + 1], z * step);
                    Vector3 v01 = new Vector3(x * step,             hmap[z + 1, x],     (z + 1) * step);
                    Vector3 v11 = new Vector3((x + 1) * step,       hmap[z + 1, x + 1], (z + 1) * step);

                    // TRI 1
                    Vector3 t0v0 = v00;
                    Vector3 t0v1 = v01;
                    Vector3 t0v2 = v11;
                    Vector3 n0   = Vector3.Cross(t0v1 - t0v0, t0v2 - t0v0).normalized;

                    verts[vi + 0] = t0v0;
                    verts[vi + 1] = t0v1;
                    verts[vi + 2] = t0v2;

                    normals[vi + 0] = n0;
                    normals[vi + 1] = n0;
                    normals[vi + 2] = n0;

                    uvs[vi + 0] = new Vector2((float)x     / resolution * tiling, (float)z     / resolution * tiling);
                    uvs[vi + 1] = new Vector2((float)x     / resolution * tiling, (float)(z+1) / resolution * tiling);
                    uvs[vi + 2] = new Vector2((float)(x+1) / resolution * tiling, (float)(z+1) / resolution * tiling);

                    tris[vi + 0] = vi + 0;
                    tris[vi + 1] = vi + 1;
                    tris[vi + 2] = vi + 2;

                    // TRI 2
                    Vector3 t1v0 = v00;
                    Vector3 t1v1 = v11;
                    Vector3 t1v2 = v10;
                    Vector3 n1   = Vector3.Cross(t1v1 - t1v0, t1v2 - t1v0).normalized;

                    verts[vi + 3] = t1v0;
                    verts[vi + 4] = t1v1;
                    verts[vi + 5] = t1v2;

                    normals[vi + 3] = n1;
                    normals[vi + 4] = n1;
                    normals[vi + 5] = n1;

                    uvs[vi + 3] = new Vector2((float)x     / resolution * tiling, (float)z     / resolution * tiling);
                    uvs[vi + 4] = new Vector2((float)(x+1) / resolution * tiling, (float)(z+1) / resolution * tiling);
                    uvs[vi + 5] = new Vector2((float)(x+1) / resolution * tiling, (float)z     / resolution * tiling);

                    tris[vi + 3] = vi + 3;
                    tris[vi + 4] = vi + 4;
                    tris[vi + 5] = vi + 5;

                    vi += 6;
                }
            }

            Mesh m = new Mesh();
            m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            m.vertices  = verts;
            m.normals   = normals;
            m.uv        = uvs;
            m.triangles = tris;

            m.RecalculateBounds();
            return m;
        }
    }
}
