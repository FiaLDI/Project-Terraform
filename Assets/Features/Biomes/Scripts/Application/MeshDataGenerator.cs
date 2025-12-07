using UnityEngine;
using Unity.Mathematics;
using Features.Biomes.Domain;
using Features.Biomes.Utility;

namespace Features.Biomes.Application
{
    public static class MeshDataGenerator
    {
        // =========================================================================
        // PUBLIC ENTRY
        // =========================================================================
        public static MeshData GenerateData(
            Vector2Int coord,
            int chunkSize,
            int resolution,
            WorldConfig world,
            bool useLowPoly)
        {
            int vertCount;
            int triCount;

            if (useLowPoly)
            {
                vertCount = resolution * resolution * 6;
                triCount  = vertCount;
            }
            else
            {
                int vertsPerLine = resolution + 1;
                vertCount = vertsPerLine * vertsPerLine;
                triCount  = resolution * resolution * 6;
            }

            MeshData data = new MeshData(vertCount, triCount);

            // 1) heightmap LOD0
            float[,] hmap0 = GenerateHeightmapLOD0(coord, chunkSize, world);

            // 2) downsample если надо
            float[,] hmap =
                (resolution == chunkSize)
                ? hmap0
                : Downsample(hmap0, chunkSize, resolution);

            // 3) UV tiling из биома
            var blend = world.GetDominantBiome(coord);
            float tiling = blend.biome != null ? blend.biome.textureTiling : 1f;

            // 4) Mesh построение
            if (useLowPoly)
                BuildLowPoly(data, hmap, resolution, chunkSize, tiling);
            else
                BuildSmooth(data, hmap, resolution, chunkSize, tiling);

            return data;
        }

        // =========================================================================
        // HEIGHTMAP — FULL RESOLUTION
        // =========================================================================
        private static float[,] GenerateHeightmapLOD0(Vector2Int coord, int chunkSize, WorldConfig world)
        {
            int res = chunkSize;
            float[,] hmap = new float[res + 1, res + 1];

            float baseX = coord.x * chunkSize;
            float baseZ = coord.y * chunkSize;

            float step = (float)chunkSize / res;

            for (int z = 0; z <= res; z++)
            {
                for (int x = 0; x <= res; x++)
                {
                    float wx = baseX + x * step;
                    float wz = baseZ + z * step;

                    float h = SampleHeightBlended(world, wx, wz);
                    if (!float.IsFinite(h)) h = 0f;

                    hmap[z, x] = h;
                }
            }
            return hmap;
        }

        // =========================================================================
        // BIOME-BLENDED HEIGHT
        // =========================================================================
        private static float SampleHeightBlended(WorldConfig world, float wx, float wz)
        {
            var blends = world.GetBiomeBlend(new Vector3(wx, 0, wz));
            if (blends == null || blends.Length == 0)
                return 0f;

            // главный биом (максимальный вес)
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

            float hMain = BiomeHeightUtility.GetHeight(main, wx, wz);

            if (main.blendStrength <= 0.001f)
                return hMain;

            // смешивание
            float sumH = hMain * best;
            float sumW = best;

            foreach (var b in blends)
            {
                if (b.biome == main) continue;

                float w = b.weight * main.blendStrength;
                sumH += BiomeHeightUtility.GetHeight(b.biome, wx, wz) * w;
                sumW += w;
            }

            return sumW > 0f ? sumH / sumW : hMain;
        }

        // =========================================================================
        // DOWNSAMPLE
        // =========================================================================
        private static float[,] Downsample(float[,] src, int srcRes, int dstRes)
        {
            int srcSize = srcRes + 1;
            int dstSize = dstRes + 1;
            float factor = (float)srcRes / dstRes;

            float[,] dst = new float[dstSize, dstSize];

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

                    float a = Mathf.Lerp(h00, h10, tx);
                    float b = Mathf.Lerp(h01, h11, tx);

                    dst[z, x] = Mathf.Lerp(a, b, tz);
                }
            }
            return dst;
        }

        // =========================================================================
        // SMOOTH MESH
        // =========================================================================
        private static void BuildSmooth(MeshData data, float[,] hmap, int resolution, int chunkSize, float tiling)
        {
            int size = resolution + 1;

            float step = (float)chunkSize / resolution;

            int vi = 0;
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    float h = hmap[z, x];
                    data.vertices[vi] = new Vector3(x * step, h, z * step);
                    data.uv[vi] = new Vector2((float)x / resolution * tiling, (float)z / resolution * tiling);
                    vi++;
                }
            }

            int ti = 0;
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int v = z * size + x;

                    data.triangles[ti++] = v;
                    data.triangles[ti++] = v + size;
                    data.triangles[ti++] = v + 1;

                    data.triangles[ti++] = v + 1;
                    data.triangles[ti++] = v + size;
                    data.triangles[ti++] = v + size + 1;
                }
            }

            data.normals = null;
        }

        // =========================================================================
        // LOW POLY MESH
        // =========================================================================
        private static void BuildLowPoly(MeshData data, float[,] hmap, int resolution, int chunkSize, float tiling)
        {
            float step = (float)chunkSize / resolution;

            int vi = 0;
            int ti = 0;

            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    Vector3 v00 = new Vector3(x * step, hmap[z, x], z * step);
                    Vector3 v10 = new Vector3((x+1)*step, hmap[z, x+1], z*step);
                    Vector3 v01 = new Vector3(x * step, hmap[z+1, x], (z+1)*step);
                    Vector3 v11 = new Vector3((x+1)*step, hmap[z+1, x+1], (z+1)*step);

                    // tri 1
                    data.vertices[vi]   = v00;
                    data.vertices[vi+1] = v01;
                    data.vertices[vi+2] = v11;

                    Vector3 n0 = Vector3.Cross(v01 - v00, v11 - v00).normalized;
                    data.normals ??= new Vector3[resolution * resolution * 6];
                    data.normals[vi] = data.normals[vi+1] = data.normals[vi+2] = n0;

                    data.uv[vi]   = new Vector2((float)x/resolution * tiling, (float)z/resolution * tiling);
                    data.uv[vi+1] = new Vector2((float)x/resolution * tiling, (float)(z+1)/resolution * tiling);
                    data.uv[vi+2] = new Vector2((float)(x+1)/resolution * tiling, (float)(z+1)/resolution * tiling);

                    data.triangles[ti++] = vi;
                    data.triangles[ti++] = vi+1;
                    data.triangles[ti++] = vi+2;

                    // tri 2
                    data.vertices[vi+3] = v00;
                    data.vertices[vi+4] = v11;
                    data.vertices[vi+5] = v10;

                    Vector3 n1 = Vector3.Cross(v11 - v00, v10 - v00).normalized;
                    data.normals[vi+3] = data.normals[vi+4] = data.normals[vi+5] = n1;

                    data.uv[vi+3] = new Vector2((float)x/resolution * tiling, (float)z/resolution * tiling);
                    data.uv[vi+4] = new Vector2((float)(x+1)/resolution * tiling, (float)(z+1)/resolution * tiling);
                    data.uv[vi+5] = new Vector2((float)(x+1)/resolution * tiling, (float)z/resolution * tiling);

                    data.triangles[ti++] = vi+3;
                    data.triangles[ti++] = vi+4;
                    data.triangles[ti++] = vi+5;

                    vi += 6;
                }
            }
        }
    }
}
