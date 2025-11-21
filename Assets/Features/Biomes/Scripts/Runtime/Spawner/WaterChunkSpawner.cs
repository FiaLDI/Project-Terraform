using UnityEngine;

public class WaterChunkSpawner
{
    private readonly Vector2Int coord;
    private readonly int chunkSize;
    private readonly BiomeConfig biome;
    private readonly Transform parent;

    public WaterChunkSpawner(Vector2Int coord, int chunkSize, BiomeConfig biome, Transform parent)
    {
        this.coord = coord;
        this.chunkSize = chunkSize;
        this.biome = biome;
        this.parent = parent;
    }

    public void Spawn()
    {
        if (!biome.useWater && !biome.generateLakes && !biome.generateRivers)
            return;

        float seaLevel = biome.seaLevel;

        // SEA (global water)
        if (biome.useWater)
        {
            SpawnWaterPlane(seaLevel, "Sea");
        }

        // Lakes
        if (biome.generateLakes)
        {
            float baseLevel = biome.heightMultiplier * biome.lakeLevel;

            float noise = Mathf.PerlinNoise(
                coord.x * biome.lakeNoiseScale,
                coord.y * biome.lakeNoiseScale
            ) * 2f - 1f;

            float finalLevel = baseLevel + noise * 0.5f;

            SpawnWaterPlane(finalLevel, "Lake");
        }

        // Rivers
        if (biome.generateRivers)
        {
            SpawnRiver();
        }
    }

    private void SpawnWaterPlane(float height, string name)
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = name;
        plane.transform.SetParent(parent);
        plane.transform.position = new Vector3(
            coord.x * chunkSize + chunkSize / 2f,
            height,
            coord.y * chunkSize + chunkSize / 2f
        );

        plane.transform.localScale = Vector3.one * (chunkSize / 10f);
        plane.GetComponent<MeshRenderer>().sharedMaterial = biome.waterMaterial;
    }

    private void SpawnRiver()
    {
        float n = Mathf.PerlinNoise(
            coord.x * biome.riverNoiseScale,
            coord.y * biome.riverNoiseScale
        );

        if (n < 0.45f || n > 0.55f)
            return;

        float height = biome.seaLevel - biome.riverDepth;

        GameObject river = GameObject.CreatePrimitive(PrimitiveType.Plane);
        river.name = "River";
        river.transform.SetParent(parent);

        float width = chunkSize * biome.riverWidth;

        river.transform.position = new Vector3(
            coord.x * chunkSize + chunkSize / 2f,
            height,
            coord.y * chunkSize + chunkSize / 2f
        );

        river.transform.localScale = new Vector3(width / 10f, 1, chunkSize / 10f);
        river.GetComponent<MeshRenderer>().sharedMaterial = biome.waterMaterial;
    }
}
