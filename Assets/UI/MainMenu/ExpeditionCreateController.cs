using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public sealed class ExpeditionCreateController : MonoBehaviour
{
    [Header("Input")]
    public TMP_InputField expeditionNameInput;

    [Header("Labels")]
    public TMP_Text titleLabel;
    public TMP_Text createButtonLabel;
    public TMP_Text cancelButtonLabel;
    public TMP_Text inputPlaceholderLabel;

    [Header("Campaign")]
    [SerializeField] private CampaignCatalogSO campaignCatalog;

    private CampaignProgressService _campaignProgress;

    private void Start()
    {
        EnsureServices();
        ApplyLabels();
    }

    public void EnterExpeditionCreate()
    {
        EnsureServices();
        ApplyLabels();

        if (expeditionNameInput != null)
            expeditionNameInput.text = GetSuggestedExpeditionName();
    }

    public void OnCreate()
    {
        EnsureServices();

        string expeditionName = expeditionNameInput != null
            ? expeditionNameInput.text.Trim()
            : string.Empty;

        _campaignProgress.CreateExpedition(expeditionName, GetDefaultStartingPlanetId());
        MainMenuFSM.Instance.Switch(MainMenuStateId.ExpeditionSelect);
    }

    public void OnCancel()
    {
        MainMenuFSM.Instance.Switch(MainMenuStateId.ExpeditionSelect);
    }

    private void EnsureServices()
    {
        if (_campaignProgress == null)
            _campaignProgress = CampaignProgressService.EnsureExists();

        if (campaignCatalog == null && CampaignRuntimeState.CurrentCatalog != null)
            campaignCatalog = CampaignRuntimeState.CurrentCatalog;
    }

    private void ApplyLabels()
    {
        if (titleLabel != null)
            titleLabel.text = "Create Expedition";

        if (createButtonLabel != null)
            createButtonLabel.text = "Create Expedition";

        if (cancelButtonLabel != null)
            cancelButtonLabel.text = "Back";

        if (inputPlaceholderLabel != null)
            inputPlaceholderLabel.text = "Expedition name";
    }

    private string GetDefaultStartingPlanetId()
    {
        if (campaignCatalog == null)
            return string.Empty;

        ExpeditionSaveData preview = new ExpeditionSaveData
        {
            shipLevel = 1
        };

        List<PlanetConfig> planets = CampaignCatalogUtility.GetAvailablePlanets(campaignCatalog, preview);
        PlanetConfig defaultPlanet = planets.FirstOrDefault() ??
            campaignCatalog.planets.FirstOrDefault(x => x != null && !string.IsNullOrWhiteSpace(x.planetId));

        return defaultPlanet != null ? defaultPlanet.planetId : string.Empty;
    }

    private string GetSuggestedExpeditionName()
    {
        IReadOnlyList<ExpeditionSaveData> expeditions = _campaignProgress != null
            ? _campaignProgress.GetExpeditions()
            : null;

        if (expeditions == null || expeditions.Count == 0)
            return "Expedition 01";

        int maxIndex = 0;
        const string prefix = "Expedition ";

        for (int i = 0; i < expeditions.Count; i++)
        {
            ExpeditionSaveData expedition = expeditions[i];
            if (expedition == null || string.IsNullOrWhiteSpace(expedition.displayName))
                continue;

            if (!expedition.displayName.StartsWith(prefix))
                continue;

            string numberPart = expedition.displayName.Substring(prefix.Length).Trim();
            if (int.TryParse(numberPart, out int parsed))
                maxIndex = Mathf.Max(maxIndex, parsed);
        }

        return $"Expedition {maxIndex + 1:00}";
    }
}
