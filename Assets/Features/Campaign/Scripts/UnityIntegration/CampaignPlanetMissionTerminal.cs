using FishNet;
using UnityEngine;

public sealed class CampaignPlanetMissionTerminal : MonoBehaviour, IInteractable
{
    [SerializeField] private CampaignCatalogSO campaignCatalog;
    [SerializeField] private string interactionPrompt = "Начать планетарную миссию";

    public string InteractionPrompt => interactionPrompt;

    public bool Interact()
    {
        if (InstanceFinder.NetworkManager == null || !InstanceFinder.NetworkManager.IsServerStarted)
        {
            Debug.LogWarning("[CampaignPlanetMissionTerminal] Only host can launch planet mission.", this);
            return false;
        }

        CampaignProgressService progress = CampaignProgressService.EnsureExists();
        ExpeditionSaveData expedition = progress != null ? progress.ActiveExpedition : null;
        if (expedition == null)
        {
            Debug.LogWarning("[CampaignPlanetMissionTerminal] Active expedition is not selected.", this);
            return false;
        }

        CampaignCatalogSO catalog = campaignCatalog != null
            ? campaignCatalog
            : CampaignRuntimeState.CurrentCatalog;

        PlanetConfig planet = CampaignCatalogUtility.FindPlanet(catalog, expedition.activePlanetId);
        if (planet == null)
        {
            Debug.LogWarning("[CampaignPlanetMissionTerminal] Active planet is not configured.", this);
            return false;
        }

        PlanetProgressData planetProgress = progress.GetOrCreatePlanetProgress(planet.planetId);
        if (planetProgress == null || !planetProgress.isPlanetMissionUnlocked)
        {
            Debug.LogWarning("[CampaignPlanetMissionTerminal] Planet mission is still locked.", this);
            return false;
        }

        if (string.IsNullOrWhiteSpace(planet.planetMissionSceneName))
        {
            Debug.LogError("[CampaignPlanetMissionTerminal] Planet mission scene name is empty.", this);
            return false;
        }

        if (!CampaignPlanetMissionBootstrap.TryPrepareMission(
                catalog,
                planet.planetId,
                resetExistingQuests: true,
                out _,
                out string failureReason))
        {
            Debug.LogError("[CampaignPlanetMissionTerminal] " + failureReason, this);
            return false;
        }

        CampaignRuntimeState.SetCatalog(catalog);
        SceneTransitionService.LoadScene(planet.planetMissionSceneName);
        return true;
    }
}
