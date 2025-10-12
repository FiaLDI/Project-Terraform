using UnityEngine;

public class BiomeGenerator : MonoBehaviour
{
    public BiomeConfig biome;
    public int chunkSize = 32; // размер чанка

    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        if (biome == null)
        {
            Debug.LogWarning("❌ BiomeConfig не назначен!");
            return;
        }

        // ✅ Установка skybox
        if (biome.skyboxMaterial != null)
        {
            RenderSettings.skybox = biome.skyboxMaterial;
            DynamicGI.UpdateEnvironment();
            Debug.Log($"🌌 Skybox для биома '{biome.biomeName}' применён.");
        }

        int width = biome.width;
        int height = biome.height;

        GameObject biomeRoot = new GameObject(biome.biomeName + "_Generated");

        for (int cz = 0; cz < height; cz += chunkSize)
        {
            for (int cx = 0; cx < width; cx += chunkSize)
            {
                int w = Mathf.Min(chunkSize, width - cx);
                int h = Mathf.Min(chunkSize, height - cz);

                GameObject chunk = GenerateChunk(cx, cz, w, h, biome);
                chunk.transform.parent = biomeRoot.transform;
            }
        }

        Debug.Log($"✅ Biome '{biome.biomeName}' сгенерирован чанками!");
    }

    private GameObject GenerateChunk(int startX, int startZ, int width, int height, BiomeConfig biome)
    {
        GameObject chunkObj = new GameObject($"Chunk_{startX}_{startZ}");

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
        int[] triangles = new int[width * height * 6];

        // вершины
        for (int z = 0, i = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                float noise = Mathf.PerlinNoise(
                    (startX + x) * biome.terrainScale * 0.01f,
                    (startZ + z) * biome.terrainScale * 0.01f
                );

                float y = 0f;

                switch (biome.terrainType)
                {
                    case TerrainType.SmoothHills:
                        y = noise * biome.heightMultiplier;
                        break;

                    case TerrainType.SharpMountains:
                        y = Mathf.Pow(noise, 3f) * biome.heightMultiplier;
                        break;

                    case TerrainType.Plateaus:
                        y = Mathf.Round(noise * 3f) / 3f * biome.heightMultiplier;
                        break;

                    case TerrainType.Craters:
                        y = (1f - Mathf.Abs(noise * 2f - 1f)) * biome.heightMultiplier;
                        break;

                    case TerrainType.Dunes:
                        float dune = Mathf.PerlinNoise((startX + x) * biome.terrainScale * 0.05f, 0f);
                        y = dune * biome.heightMultiplier * 0.5f;
                        break;

                    case TerrainType.Islands:
                        float dist = Vector2.Distance(new Vector2(startX + x, startZ + z),
                                                      new Vector2(biome.width / 2f, biome.height / 2f));
                        float gradient = Mathf.Clamp01(1f - dist / (biome.width / 2f));
                        y = noise * biome.heightMultiplier * gradient;
                        break;

                    case TerrainType.Canyons:
                        float canyon = Mathf.Abs(Mathf.PerlinNoise((startX + x) * 0.05f, 0f) - 0.5f) * 2f;
                        y = noise * biome.heightMultiplier * canyon;
                        break;

                    case TerrainType.FractalMountains:
                        y = RidgedNoise(startX + x, startZ + z,
                                        biome.terrainScale * 0.01f,
                                        biome.fractalOctaves,
                                        biome.fractalPersistence,
                                        biome.fractalLacunarity) * biome.heightMultiplier;
                        break;
                }

                vertices[i] = new Vector3(startX + x, y, startZ + z);
            }
        }

        // треугольники
        for (int z = 0, vert = 0, tris = 0; z < height; z++, vert++)
        {
            for (int x = 0; x < width; x++, vert++, tris += 6)
            {
                triangles[tris + 0] = vert;
                triangles[tris + 1] = vert + width + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + width + 1;
                triangles[tris + 5] = vert + width + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        MeshFilter mf = chunkObj.AddComponent<MeshFilter>();
        MeshRenderer mr = chunkObj.AddComponent<MeshRenderer>();
        MeshCollider mc = chunkObj.AddComponent<MeshCollider>();

        mf.sharedMesh = mesh;
        mr.sharedMaterial = biome.groundMaterial;
        mc.sharedMesh = mesh;

        return chunkObj;
    }

    // ✅ Фрактальный ridged-шум
    private float RidgedNoise(float x, float z, float scale, int octaves, float persistence, float lacunarity)
    {
        float total = 0f;
        float frequency = 1f;
        float amplitude = 1f;
        float maxValue = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float n = Mathf.PerlinNoise(x * scale * frequency, z * scale * frequency);
            n = 1f - Mathf.Abs(n * 2f - 1f); // ridged
            total += n * amplitude;

            maxValue += amplitude;
            amplitude *= persistence;   // 0.5–0.7
            frequency *= lacunarity;    // 2.0
        }

        return total / maxValue;
    }
}
