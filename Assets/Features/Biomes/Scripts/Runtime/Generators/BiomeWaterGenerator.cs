using UnityEngine;

public static class BiomeWaterGenerator
{
    public static GameObject CreateWaterPlane(BiomeConfig biome, Transform parent)
    {
        if (!biome.useWater || biome.waterMaterial == null)
            return null;

        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.name = "WaterPlane";

        float sizeX = biome.width;
        float sizeZ = biome.height;

        water.transform.parent = parent;
        water.transform.position = new Vector3(sizeX / 2f, biome.seaLevel, sizeZ / 2f);
        water.transform.localScale = new Vector3(sizeX / 10f, 1f, sizeZ / 10f); // Plane 10x10 юнитов

        var mr = water.GetComponent<MeshRenderer>();
        mr.sharedMaterial = biome.waterMaterial;

        var col = water.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);

        return water;
    }
}
