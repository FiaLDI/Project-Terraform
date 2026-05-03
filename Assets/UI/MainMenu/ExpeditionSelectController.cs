using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ExpeditionSelectController : MonoBehaviour
{
    [Header("List")]
    public Transform expeditionListRoot;
    public CharacterCardView expeditionCardPrefab;
    public Button continueButton;
    public Button deleteButton;

    [Header("Labels")]
    public TMP_Text titleLabel;
    public TMP_Text continueButtonLabel;
    public TMP_Text createButtonLabel;
    public TMP_Text deleteButtonLabel;

    [Header("Campaign")]
    [SerializeField] private CampaignCatalogSO campaignCatalog;

    private CampaignProgressService _campaignProgress;
    private readonly List<CharacterCardView> _cards = new();
    private readonly List<ExpeditionSaveData> _expeditions = new();
    private int _selectedIndex = -1;

    private void Start()
    {
        EnsureServices();
        ApplyLabels();
    }

    public void EnterExpeditionSelect()
    {
        EnsureServices();
        ApplyLabels();
        RefreshList();
    }

    public void RefreshList()
    {
        EnsureServices();
        ClearCards();
        _expeditions.Clear();

        IReadOnlyList<ExpeditionSaveData> expeditions = _campaignProgress.GetExpeditions();
        if (expeditions != null)
            _expeditions.AddRange(expeditions.Where(x => x != null));

        _selectedIndex = GetActiveExpeditionIndex();

        for (int i = 0; i < _expeditions.Count; i++)
        {
            ExpeditionSaveData expedition = _expeditions[i];
            CharacterCardView card = Instantiate(expeditionCardPrefab, expeditionListRoot);
            card.SetupExpedition(
                expedition,
                GetActivePlanetLabel(expedition),
                GetExpeditionProgressLabel(expedition),
                i,
                SelectExpedition,
                _selectedIndex);
            _cards.Add(card);
        }

        UpdateButtons();
        UpdateCardSelection();
    }

    public void OnCreateNew()
    {
        MainMenuFSM.Instance.Switch(MainMenuStateId.ExpeditionCreate);
    }

    public void OnBack()
    {
        MainMenuFSM.Instance.Switch(MainMenuStateId.CharacterSelect);
    }

    public void OnPlay()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _expeditions.Count)
            return;

        _campaignProgress.SetActiveExpedition(_expeditions[_selectedIndex]);
        MainMenuFSM.Instance.Switch(MainMenuStateId.StartGame);
    }

    public void OnDelete()
    {
        if (_selectedIndex < 0 || _selectedIndex >= _expeditions.Count)
            return;

        _campaignProgress.DeleteExpedition(_expeditions[_selectedIndex].expeditionId);
        _selectedIndex = -1;
        RefreshList();
    }

    private void SelectExpedition(int index)
    {
        _selectedIndex = index;

        if (index >= 0 && index < _expeditions.Count)
            _campaignProgress.SetActiveExpedition(_expeditions[index]);

        UpdateButtons();
        UpdateCardSelection();
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
            titleLabel.text = "Select expedition";

        if (continueButtonLabel != null)
            continueButtonLabel.text = "Continue";

        if (createButtonLabel != null)
            createButtonLabel.text = "New Expedition";

        if (deleteButtonLabel != null)
            deleteButtonLabel.text = "Delete";
    }

    private void UpdateButtons()
    {
        bool valid = _selectedIndex >= 0;

        if (continueButton != null)
            continueButton.interactable = valid;

        if (deleteButton != null)
            deleteButton.interactable = valid;
    }

    private void UpdateCardSelection()
    {
        for (int i = 0; i < _cards.Count; i++)
            _cards[i].SetSelected(i == _selectedIndex);
    }

    private void ClearCards()
    {
        foreach (CharacterCardView card in _cards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        _cards.Clear();
    }

    private int GetActiveExpeditionIndex()
    {
        if (_campaignProgress == null || _campaignProgress.ActiveExpedition == null)
            return _expeditions.Count > 0 ? 0 : -1;

        string activeId = _campaignProgress.ActiveExpedition.expeditionId;
        for (int i = 0; i < _expeditions.Count; i++)
        {
            if (_expeditions[i] != null && _expeditions[i].expeditionId == activeId)
                return i;
        }

        return _expeditions.Count > 0 ? 0 : -1;
    }

    private string GetActivePlanetLabel(ExpeditionSaveData expedition)
    {
        if (expedition == null || string.IsNullOrWhiteSpace(expedition.activePlanetId))
            return "Planet: none";

        PlanetConfig planet = CampaignCatalogUtility.FindPlanet(campaignCatalog, expedition.activePlanetId);
        string planetName = planet != null && !string.IsNullOrWhiteSpace(planet.displayName)
            ? planet.displayName
            : expedition.activePlanetId;

        return "Planet: " + planetName;
    }

    private string GetExpeditionProgressLabel(ExpeditionSaveData expedition)
    {
        if (expedition == null)
            return "No progress yet";

        PlanetConfig activePlanet = CampaignCatalogUtility.FindPlanet(campaignCatalog, expedition.activePlanetId);
        if (activePlanet == null)
            return "No progress yet";

        List<Biomes.Data.BiomeConfig> biomes = CampaignCatalogUtility.GetPlanetBiomes(activePlanet);
        if (biomes.Count == 0)
            return "No biomes";

        int targetThreat = Mathf.Min(2, CampaignCatalogUtility.GetShipThreatCap(campaignCatalog, expedition.shipLevel));
        int completed = 0;

        PlanetProgressData planetProgress = expedition.planets != null
            ? expedition.planets.FirstOrDefault(x => x != null && x.planetId == activePlanet.planetId)
            : null;

        for (int i = 0; i < biomes.Count; i++)
        {
            string biomeId = biomes[i].biomeID;
            BiomeThreatProgressData biomeProgress = planetProgress != null && planetProgress.biomeThreats != null
                ? planetProgress.biomeThreats.FirstOrDefault(x => x != null && x.biomeId == biomeId)
                : null;

            if (biomeProgress != null &&
                biomeProgress.completedThreatLevels != null &&
                biomeProgress.completedThreatLevels.Contains(targetThreat))
            {
                completed++;
            }
        }

        return $"{completed}/{biomes.Count} biomes at T{targetThreat}";
    }
}
