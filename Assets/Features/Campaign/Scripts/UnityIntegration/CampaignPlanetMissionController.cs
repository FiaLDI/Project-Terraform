using System.Collections;
using Features.Player.UnityIntegration;
using FishNet;
using UnityEngine;

public sealed class CampaignPlanetMissionController : MonoBehaviour
{
    [SerializeField] private CampaignCatalogSO campaignCatalog;
    [SerializeField] private bool resetPlayerQuestsOnStart = true;
    [SerializeField] private bool returnPlayersToHubOnComplete = true;
    [SerializeField] private float bootstrapDelay = 0.5f;
    [SerializeField] private float pollInterval = 0.5f;

    private bool bootstrapped;
    private bool completionApplied;
    private string activePlanetId;
    private string activeMissionId;

    private void Start()
    {
        StartCoroutine(ServerRoutine());
    }

    private IEnumerator ServerRoutine()
    {
        yield return new WaitUntil(IsServerStarted);
        yield return new WaitForSeconds(bootstrapDelay);

        CampaignCatalogSO catalog = ResolveCatalog();
        CampaignProgressService progress = CampaignProgressService.EnsureExists();

        if (catalog == null || progress == null)
            yield break;

        if (CampaignPlanetMissionRuntimeState.HasActiveMission)
        {
            activePlanetId = CampaignPlanetMissionRuntimeState.ActivePlanetId;
            activeMissionId = CampaignPlanetMissionRuntimeState.ActiveMissionId;
        }
        else
        {
            if (progress.ActiveExpedition == null)
                yield break;

            activePlanetId = progress.ActiveExpedition.activePlanetId;
            PlanetConfig expeditionPlanet = CampaignCatalogUtility.FindPlanet(catalog, activePlanetId);
            activeMissionId = expeditionPlanet != null ? expeditionPlanet.planetMissionId : string.Empty;
        }

        PlanetConfig activePlanet = CampaignCatalogUtility.FindPlanet(catalog, activePlanetId);

        if (activePlanet == null || string.IsNullOrWhiteSpace(activeMissionId))
        {
            Debug.LogWarning("[CampaignPlanetMissionController] Active planet mission is not configured.", this);
            yield break;
        }

        CampaignRuntimeState.SetCatalog(catalog);

        while (!completionApplied)
        {
            EnsureMissionAssigned();

            if (bootstrapped && AreAllPlayersMissionCompleted())
                CompleteMission();

            yield return new WaitForSeconds(pollInterval);
        }
    }

    private void EnsureMissionAssigned()
    {
        PlayerQuestComponent[] questComponents = UnityEngine.Object.FindObjectsByType<PlayerQuestComponent>(FindObjectsSortMode.None);
        if (questComponents == null || questComponents.Length == 0)
            return;

        for (int i = 0; i < questComponents.Length; i++)
        {
            PlayerQuestComponent quests = questComponents[i];
            if (quests == null)
                continue;

            if (resetPlayerQuestsOnStart && !bootstrapped)
                quests.ClearAll();

            if (!quests.HasQuest(activeMissionId))
                quests.GiveQuests(new System.Collections.Generic.List<string> { activeMissionId });
        }

        bootstrapped = true;
    }

    private bool AreAllPlayersMissionCompleted()
    {
        PlayerQuestComponent[] questComponents = UnityEngine.Object.FindObjectsByType<PlayerQuestComponent>(FindObjectsSortMode.None);
        if (questComponents == null || questComponents.Length == 0)
            return false;

        for (int i = 0; i < questComponents.Length; i++)
        {
            PlayerQuestComponent quests = questComponents[i];
            if (quests == null)
                continue;

            if (!quests.HasQuest(activeMissionId) || !quests.IsQuestCompleted(activeMissionId))
                return false;
        }

        return true;
    }

    private void CompleteMission()
    {
        if (completionApplied)
            return;

        completionApplied = true;
        CampaignProgressService.EnsureExists()?.MarkPlanetMissionCompleted(activePlanetId);
        CampaignPlanetMissionRuntimeState.Clear();

        if (returnPlayersToHubOnComplete)
            SceneTransitionService.ReturnAllPlayersToHub();
    }

    private CampaignCatalogSO ResolveCatalog()
    {
        if (campaignCatalog != null)
            return campaignCatalog;

        return CampaignRuntimeState.CurrentCatalog;
    }

    private static bool IsServerStarted()
    {
        return InstanceFinder.NetworkManager != null &&
               InstanceFinder.NetworkManager.IsServerStarted;
    }
}
