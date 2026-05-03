using System.Collections.Generic;
using UnityEngine;

public static class CampaignPlanetMissionBootstrap
{
    public static bool TryPrepareMission(
        CampaignCatalogSO catalog,
        string planetId,
        bool resetExistingQuests,
        out PlanetConfig planet,
        out string failureReason)
    {
        planet = CampaignCatalogUtility.FindPlanet(catalog, planetId);
        failureReason = string.Empty;

        if (catalog == null)
        {
            failureReason = "Campaign Catalog is not assigned.";
            return false;
        }

        if (planet == null)
        {
            failureReason = "Planet is not configured.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(planet.planetMissionId))
        {
            failureReason = "Planet mission id is empty.";
            return false;
        }

        PlayerQuestComponent[] questComponents =
            Object.FindObjectsByType<PlayerQuestComponent>(FindObjectsSortMode.None);

        if (questComponents == null || questComponents.Length == 0)
        {
            failureReason = "No PlayerQuestComponent found for mission bootstrap.";
            return false;
        }

        for (int i = 0; i < questComponents.Length; i++)
        {
            PlayerQuestComponent quests = questComponents[i];
            if (quests == null)
                continue;

            if (resetExistingQuests)
                quests.ClearAll();

            if (!quests.HasQuest(planet.planetMissionId))
                quests.GiveQuests(new List<string> { planet.planetMissionId });
        }

        CampaignPlanetMissionRuntimeState.Set(planet.planetId, planet.planetMissionId);
        Debug.Log($"[CampaignPlanetMissionBootstrap] Prepared mission '{planet.planetMissionId}' for planet '{planet.planetId}'.");
        return true;
    }
}
