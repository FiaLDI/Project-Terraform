using UnityEngine;

public class BiomeGenerator : MonoBehaviour
{
    public BiomeConfig biome;

    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        if (biome == null) return;

        int width = biome.width;
        int height = biome.height;

        GameObject biomeRoot = new GameObject(biome.biomeName + "_Generated");

        int chunkSize = 32; // размер чанка
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
                float y = Mathf.PerlinNoise(
                    (startX + x) * biome.terrainScale * 0.01f,
                    (startZ + z) * biome.terrainScale * 0.01f
                ) * biome.heightMultiplier;

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
}
