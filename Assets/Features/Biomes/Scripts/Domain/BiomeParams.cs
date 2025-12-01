using Unity.Mathematics;
using System;

namespace Features.Biomes.Domain
{
    [Serializable]
    public struct BiomeParams
    {
        public int biomeId;

        // ===== Terrain =====
        public int terrainType;
        public float terrainScale;
        public float heightMultiplier;

        public int fractalOctaves;
        public float fractalPersistence;
        public float fractalLacunarity;

        public float islandWidth;
        public float islandHeight;

        // ===== Water =====
        public int useWater;
        public int waterType;
        public float seaLevel;

        public int generateLakes;
        public float lakeLevel;
        public float lakeNoiseScale;

        public int generateRivers;
        public float riverNoiseScale;
        public float riverWidth;
        public float riverDepth;

        // ===== Environment =====
        public float environmentDensity;
        public float environmentMinSlope;
        public float environmentMaxSlope;
        public float environmentMinScale;
        public float environmentMaxScale;
        public int environmentAlignToNormal;
        public int environmentRandomYRot;

        public int envRuleStart;
        public int envRuleCount;

        // ===== Resources =====
        public float resourceDensity;
        public float resourceSpawnYOffset;
        public float resourceEdgeFalloff;

        public int resRuleStart;
        public int resRuleCount;

        // ===== Enemies =====
        public float enemyDensity;
        public float enemyRespawnDelay;

        public int enemyRuleStart;
        public int enemyRuleCount;

        // ===== Quests (NEW) =====
        public float questSpawnChance;
        public int questTargetsMin;
        public int questTargetsMax;

        public int questRuleStart;
        public int questRuleCount;
    }
}
