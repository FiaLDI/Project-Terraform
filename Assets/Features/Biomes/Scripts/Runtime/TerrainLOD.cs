using UnityEngine;

public class TerrainLOD
{
    private readonly Vector2Int coord;
    private readonly BiomeConfig biome;
    private readonly WorldConfig world;
    private readonly int size;
    private readonly Transform parent;

    private GameObject lodRoot;

    public TerrainLOD(Vector2Int coord, BiomeConfig biome, WorldConfig world, int size, Transform parent)
    {
        this.coord = coord;
        this.biome = biome;
        this.world = world;
        this.size = size;
        this.parent = parent;

        lodRoot = new GameObject($"Terrain_LOD_{coord.x}_{coord.y}");
        lodRoot.transform.SetParent(parent, false);
    }

    public void Generate()
    {
        // ✔ Всегда создаём LODGroup
        LODGroup group = lodRoot.AddComponent<LODGroup>();

        // Временный список рендереров для LODов
        LOD[] lods = new LOD[4];

        lods[0] = CreateLOD(32, 0.6f);
        lods[1] = CreateLOD(16, 0.35f);
        lods[2] = CreateLOD(8, 0.2f);
        lods[3] = CreateLOD(4, 0.05f);

        // ✔ Только здесь вызываем SetLODs — когда все LOD-объекты созданы
        group.SetLODs(lods);
        group.RecalculateBounds();
    }

    private LOD CreateLOD(int resolution, float transition)
    {
        Mesh mesh = TerrainMeshGenerator.GenerateMeshSync(coord, size, resolution, world);

        GameObject go = new GameObject($"LOD_{resolution}");
        go.transform.SetParent(lodRoot.transform, false);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = biome.groundMaterial;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        // ✔ Коллайдер ТОЛЬКО на фулл-LOD
        if (resolution == 32)
        {
            MeshCollider mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
        }

        return new LOD(transition, new Renderer[] { mr });
    }
}
