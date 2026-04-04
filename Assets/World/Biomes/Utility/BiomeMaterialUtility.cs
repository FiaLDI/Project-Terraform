using UnityEngine;
using Biomes.Data;

namespace Biomes.Utility { 
    public static class BiomeMaterialUtility
    {
        public static void ApplyBiomeMaterial(
            MeshRenderer mr,
            BiomeConfig biome,
            WorldConfig world)
        {
            if (mr == null || biome == null || world == null)
                return;

            if (world.worldGroundMaterial != null)
                mr.sharedMaterial = world.worldGroundMaterial;

            var mpb = new MaterialPropertyBlock();

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
}