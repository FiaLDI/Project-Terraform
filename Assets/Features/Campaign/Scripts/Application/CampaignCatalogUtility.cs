using System.Collections.Generic;
using System.Linq;
using Biomes.Data;
using Features.Quests.Data;
using UnityEngine;

public static class CampaignCatalogUtility
{
    public static HashSet<string> GetUnlockedPlanetIds(CampaignCatalogSO catalog, int shipLevel)
    {
        var result = new HashSet<string>();
        if (catalog == null || catalog.shipLevels == null)
            return result;

        int normalizedShipLevel = Mathf.Max(1, shipLevel);

        foreach (ShipLevelConfig config in catalog.shipLevels
                     .Where(x => x != null)
                     .Where(x => x.shipLevel <= normalizedShipLevel)
                     .OrderBy(x => x.shipLevel))
        {
            if (config.unlockedPlanetIds == null)
                continue;

            for (int i = 0; i < config.unlockedPlanetIds.Count; i++)
            {
                string planetId = config.unlockedPlanetIds[i];
                if (!string.IsNullOrWhiteSpace(planetId))
                    result.Add(planetId);
            }
        }

        return result;
    }

    public static ShipLevelConfig GetShipLevelConfig(CampaignCatalogSO catalog, int shipLevel)
    {
        if (catalog == null || catalog.shipLevels == null)
            return null;

        return catalog.shipLevels
            .Where(x => x != null)
            .OrderBy(x => x.shipLevel)
            .LastOrDefault(x => x.shipLevel <= Mathf.Max(1, shipLevel));
    }

    public static ShipLevelConfig GetNextShipLevelConfig(CampaignCatalogSO catalog, int shipLevel)
    {
        if (catalog == null || catalog.shipLevels == null)
            return null;

        return catalog.shipLevels
            .Where(x => x != null)
            .Where(x => x.shipLevel > Mathf.Max(1, shipLevel))
            .OrderBy(x => x.shipLevel)
            .FirstOrDefault();
    }

    public static List<PlanetConfig> GetAvailablePlanets(
        CampaignCatalogSO catalog,
        ExpeditionSaveData expedition)
    {
        var result = new List<PlanetConfig>();
        if (catalog == null || expedition == null || catalog.planets == null)
            return result;

        int shipLevel = Mathf.Max(1, expedition.shipLevel);
        HashSet<string> unlockedPlanetIds = GetUnlockedPlanetIds(catalog, shipLevel);
        if (unlockedPlanetIds.Count == 0)
            return result;

        return catalog.planets
            .Where(x => x != null)
            .Where(x => !string.IsNullOrWhiteSpace(x.planetId))
            .Where(x => x.worldConfig != null)
            .Where(x => x.requiredShipLevel <= shipLevel)
            .Where(x => unlockedPlanetIds.Contains(x.planetId))
            .ToList();
    }

    public static List<BiomeConfig> GetPlanetBiomes(PlanetConfig planet)
    {
        var result = new List<BiomeConfig>();
        if (planet == null || planet.worldConfig == null || planet.worldConfig.biomes == null)
            return result;

        return planet.worldConfig.biomes
            .Where(x => x != null && x.config != null)
            .Select(x => x.config)
            .Where(x => !string.IsNullOrWhiteSpace(x.biomeID))
            .GroupBy(x => x.biomeID)
            .Select(x => x.First())
            .ToList();
    }

    public static PlanetConfig FindPlanet(CampaignCatalogSO catalog, string planetId)
    {
        if (catalog == null || catalog.planets == null || string.IsNullOrWhiteSpace(planetId))
            return null;

        return catalog.planets.FirstOrDefault(x => x != null && x.planetId == planetId);
    }

    public static string GetWorldConfigId(PlanetConfig planet)
    {
        return planet != null && planet.worldConfig != null
            ? planet.worldConfig.name
            : string.Empty;
    }

    public static string GetBiomeId(BiomeConfig biome)
    {
        return biome != null ? biome.biomeID : string.Empty;
    }

    public static int GetShipThreatCap(CampaignCatalogSO catalog, int shipLevel)
    {
        ShipLevelConfig config = GetShipLevelConfig(catalog, shipLevel);
        return config != null ? Mathf.Max(1, config.maxThreatLevel) : 1;
    }

    public static List<QuestAsset> GetQuestPoolFromBiome(BiomeConfig biome)
    {
        var result = new List<QuestAsset>();
        if (biome == null || biome.possibleQuests == null)
            return result;

        return biome.possibleQuests
            .Where(x => x != null && x.questAsset != null)
            .Select(x => x.questAsset)
            .Distinct()
            .ToList();
    }

    public static int GetMaxSelectableThreat(
        CampaignCatalogSO catalog,
        CampaignProgressService progressService,
        PlanetConfig planet,
        BiomeConfig biome)
    {
        if (progressService == null || progressService.ActiveExpedition == null || planet == null || biome == null)
            return 1;

        int unlocked = progressService.GetMaxUnlockedThreat(planet.planetId, biome.biomeID);
        int shipCap = GetShipThreatCap(catalog, progressService.ShipLevel);
        return Mathf.Max(1, Mathf.Min(unlocked, shipCap));
    }

    public static bool CanUnlockPlanetMission(
        PlanetConfig planet,
        CampaignProgressService progressService,
        string biomeId = null)
    {
        if (planet == null || progressService == null || progressService.ActiveExpedition == null)
            return false;

        if (!string.IsNullOrWhiteSpace(biomeId))
        {
            BiomeThreatProgressData progress = progressService.GetOrCreateBiomeProgress(
                planet.planetId,
                biomeId);

            return progress != null &&
                   progress.completedThreatLevels != null &&
                   progress.completedThreatLevels.Contains(planet.planetMissionUnlockThreatLevel);
        }

        List<BiomeConfig> biomes = GetPlanetBiomes(planet);
        if (biomes.Count == 0)
            return false;

        for (int i = 0; i < biomes.Count; i++)
        {
            BiomeConfig biome = biomes[i];
            BiomeThreatProgressData progress = progressService.GetOrCreateBiomeProgress(
                planet.planetId,
                biome.biomeID);

            if (progress == null ||
                progress.completedThreatLevels == null ||
                !progress.completedThreatLevels.Contains(planet.planetMissionUnlockThreatLevel))
            {
                return false;
            }
        }

        return true;
    }
}
