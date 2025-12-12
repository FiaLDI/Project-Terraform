using UnityEngine;
using Features.Biomes.Domain;

public static class BiomeMaterialUtility
{
    public static void ApplyBiomeMaterial(
        MeshRenderer mr,
        BiomeConfig biome,
        WorldConfig world)
    {
        if (mr == null || biome == null || world == null)
            return;

        // 1. глобальный материал мира
        if (world.worldGroundMaterial != null)
            mr.sharedMaterial = world.worldGroundMaterial;

        // 2. параметры биома
        var mpb = new MaterialPropertyBlock();

        // Пример параметров — добавь свои
        mpb.SetColor("_GroundColor", biome.groundColor);
        mpb.SetFloat("_GroundSmoothness", biome.groundSmoothness);
        mpb.SetFloat("_GroundMetallic", biome.groundMetallic);
        mpb.SetFloat("_TilingMultiplier", biome.textureTilingMultiplier);

        if (biome.biomeAlbedo != null)
            mpb.SetTexture("_BiomeAlbedo", biome.biomeAlbedo);

        if (biome.biomeNormal != null)
            mpb.SetTexture("_BiomeNormal", biome.biomeNormal);

        mr.SetPropertyBlock(mpb);
    }
}
