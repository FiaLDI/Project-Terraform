using System.Collections.Generic;
using System.Linq;
using Biomes.Data;
using FishNet;
using Features.Input;
using Features.Quests.Data;
using Features.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class WorldGeneratorUI : PlayerBoundUIView, IUIScreen
{
    private sealed class RegionRuntime
    {
        public PlanetConfig planet;
        public WorldSelectionEntry visualEntry;
        public WorldRegionButtonView view;
    }

    private sealed class QuestRuntime
    {
        public QuestAsset quest;
        public WorldQuestButtonView view;
    }

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Main Controls")]
    [SerializeField] private Button generateWorldButton;
    [SerializeField] private Button closeButton;

    [Header("Map")]
    [SerializeField] private RectTransform mapRegionsRoot;
    [SerializeField] private WorldRegionButtonView regionButtonPrefab;

    [Header("Campaign Planet List")]
    [SerializeField] private Vector2 campaignPlanetButtonSize = new(260f, 88f);
    [SerializeField] private float campaignPlanetButtonSpacing = 96f;
    [SerializeField] private float campaignPlanetButtonStartY = -20f;
    [SerializeField] private float campaignPlanetButtonX = 0f;

    [Header("Details")]
    [SerializeField] private TextMeshProUGUI selectedWorldTitle;
    [SerializeField] private TextMeshProUGUI selectedWorldDescription;

    [Header("Quest List")]
    [SerializeField] private RectTransform questListRoot;
    [SerializeField] private WorldQuestButtonView questButtonPrefab;
    [SerializeField] private TextMeshProUGUI selectedQuestTitle;
    [SerializeField] private TextMeshProUGUI selectedQuestDescription;

    [Header("Data")]
    [SerializeField] private WorldSelectionCatalog worldSelectionCatalog;
    [SerializeField] private CampaignCatalogSO campaignCatalog;

    [Header("Fallback Selector")]
    [SerializeField] private Button worldConfigButton;
    [SerializeField] private TextMeshProUGUI worldConfigButtonLabel;

    [Header("Difficulty")]
    [SerializeField] private Button difficultyButton;
    [SerializeField] private TextMeshProUGUI difficultyButtonLabel;

    [Header("Planet Mission")]
    [SerializeField] private Button launchPlanetMissionButton;
    [SerializeField] private TextMeshProUGUI launchPlanetMissionButtonLabel;

    public InputMode Mode => InputMode.Dialog;

    private readonly List<RegionRuntime> regionViews = new();
    private readonly List<QuestRuntime> questViews = new();

    private PolygonGlowButtonGroup regionGroup;
    private int selectedWorldIndex;
    private int selectedDifficulty = WorldRunBalance.DefaultDifficulty;
    private QuestAsset selectedQuest;
    private PlanetConfig selectedPlanet;
    private BiomeConfig selectedBiome;
    private bool initialized;
    private bool isGeneratingWorld;

    private bool UseCampaignFlow => campaignCatalog != null;

    protected override void OnEnable()
    {
        base.OnEnable();
        UIRegistry.I?.Register(this);
        Initialize();
        ResetSubmissionState();
        root.SetActive(false);
    }

    protected override void OnDisable()
    {
        UIRegistry.I?.Unregister(this);
        base.OnDisable();
    }

    protected override void OnPlayerBound(GameObject player)
    {
        ResetSubmissionState();
        root.SetActive(false);
    }

    public void Show()
    {
        Initialize();
        EnsureCampaignReady();
        RebuildWorldButtons();
        ResetSubmissionState();
        RefreshAll();
        root.SetActive(true);
        InputModeManager.I.SetMode(Mode);
    }

    public void Hide()
    {
        ResetSubmissionState();
        root.SetActive(false);
        InputModeManager.I.SetMode(InputMode.Gameplay);
    }

    public void Open()
    {
        UIStackManager.I.Push(this);
    }

    public void OnCloseClicked()
    {
        UIStackManager.I.Pop();
    }

    public void OnGenerateWorldClicked()
    {
        if (isGeneratingWorld)
        {
            Debug.LogWarning("WorldGeneratorUI: duplicate generate click ignored", this);
            return;
        }

        if (BoundPlayer == null)
        {
            Debug.LogError("WorldGeneratorUI: BoundPlayer is null");
            return;
        }

        string worldConfigId = GetSelectedWorldConfigId();
        if (string.IsNullOrWhiteSpace(worldConfigId))
        {
            Debug.LogWarning("WorldGeneratorUI: no world config selected", this);
            return;
        }

        isGeneratingWorld = true;

        if (generateWorldButton != null)
            generateWorldButton.interactable = false;

        LoadingScreenService.ShowWorld(GetSelectedWorldConfig(), "Generating procedural world...");

        int difficulty = GetSelectedDifficulty();
        List<string> questIds = GetSelectedQuestIds();
        List<string> chainIds = GetSelectedChainIds();

        if (UseCampaignFlow)
        {
            CampaignRunContext.EnsureExists().Set(
                selectedPlanet != null ? selectedPlanet.planetId : string.Empty,
                selectedBiome != null ? selectedBiome.biomeID : string.Empty,
                worldConfigId,
                difficulty,
                GetShipThreatCap());
        }

        var net = BoundPlayer.GetComponent<PlayerSessionNetwork>();
        net.RequestWorldServerRpc(worldConfigId, difficulty, questIds, chainIds);

        UIStackManager.I?.Clear();
    }

    public void OnCycleWorldConfigClicked()
    {
        if (UseCampaignFlow)
        {
            CycleBiomeSelection();
            return;
        }

        if (regionViews.Count == 0)
            return;

        selectedWorldIndex = (selectedWorldIndex + 1) % regionViews.Count;
        RefreshAll();
    }

    public void OnCycleDifficultyClicked()
    {
        if (UseCampaignFlow)
        {
            int max = GetMaxSelectableThreat();
            selectedDifficulty++;

            if (selectedDifficulty > max)
                selectedDifficulty = 1;

            RefreshDifficultyLabel();
            return;
        }

        selectedDifficulty++;

        if (selectedDifficulty > WorldRunBalance.MaxDifficulty)
            selectedDifficulty = WorldRunBalance.MinDifficulty;

        RefreshDifficultyLabel();
    }

    public void OnLaunchPlanetMissionClicked()
    {
        if (!UseCampaignFlow)
            return;

        if (isGeneratingWorld)
        {
            Debug.LogWarning("WorldGeneratorUI: planet mission launch ignored while busy", this);
            return;
        }

        PlanetProgressData existingProgress = CampaignProgressService.I != null && selectedPlanet != null
            ? CampaignProgressService.I.GetOrCreatePlanetProgress(selectedPlanet.planetId)
            : null;

        if (existingProgress != null && existingProgress.isPlanetMissionCompleted)
        {
            RefreshPlanetMissionButton();
            return;
        }

        if (!TryGetSelectedPlanetMission(out PlanetConfig planet, out PlanetProgressData progress, out string failureReason))
        {
            if (!string.IsNullOrWhiteSpace(failureReason))
                Debug.LogWarning("[WorldGeneratorUI] " + failureReason, this);

            RefreshPlanetMissionButton();
            return;
        }

        if (progress != null && progress.isPlanetMissionCompleted)
        {
            RefreshPlanetMissionButton();
            return;
        }

        if (InstanceFinder.NetworkManager == null || !InstanceFinder.NetworkManager.IsServerStarted)
        {
            Debug.LogWarning("[WorldGeneratorUI] Only host can launch planet missions.", this);
            RefreshPlanetMissionButton();
            return;
        }

        isGeneratingWorld = true;

        if (generateWorldButton != null)
            generateWorldButton.interactable = false;

        if (launchPlanetMissionButton != null)
            launchPlanetMissionButton.interactable = false;

        LoadingScreenService.Show(
            GetPlanetDisplayName(planet, selectedWorldIndex),
            "Loading planet mission...");

        if (!CampaignPlanetMissionBootstrap.TryPrepareMission(
                campaignCatalog,
                planet.planetId,
                resetExistingQuests: true,
                out _,
                out string bootstrapFailure))
        {
            isGeneratingWorld = false;

            if (generateWorldButton != null)
                generateWorldButton.interactable = true;

            RefreshPlanetMissionButton();
            Debug.LogError("[WorldGeneratorUI] " + bootstrapFailure, this);
            return;
        }

        CampaignRuntimeState.SetCatalog(campaignCatalog);
        SceneTransitionService.LoadScene(planet.planetMissionSceneName);
        UIStackManager.I?.Clear();
    }

    private void Initialize()
    {
        if (initialized)
            return;

        initialized = true;
        EnsureCampaignReady();
        RegisterLoadingBackgrounds();
        ResolveDifficultyUiReferences();

        if (generateWorldButton != null)
        {
            generateWorldButton.onClick.RemoveListener(OnGenerateWorldClicked);
            generateWorldButton.onClick.AddListener(OnGenerateWorldClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseClicked);
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        if (worldConfigButton != null)
        {
            worldConfigButton.onClick.RemoveListener(OnCycleWorldConfigClicked);
            worldConfigButton.onClick.AddListener(OnCycleWorldConfigClicked);
        }

        if (difficultyButton != null)
        {
            difficultyButton.onClick.RemoveListener(OnCycleDifficultyClicked);
            difficultyButton.onClick.AddListener(OnCycleDifficultyClicked);
        }

        if (launchPlanetMissionButton != null)
        {
            launchPlanetMissionButton.onClick.RemoveListener(OnLaunchPlanetMissionClicked);
            launchPlanetMissionButton.onClick.AddListener(OnLaunchPlanetMissionClicked);
        }

        if (difficultyButtonLabel != null)
            difficultyButtonLabel.raycastTarget = false;

        if (worldConfigButtonLabel != null)
            worldConfigButtonLabel.raycastTarget = false;

        if (launchPlanetMissionButtonLabel != null)
            launchPlanetMissionButtonLabel.raycastTarget = false;

        if (mapRegionsRoot != null)
        {
            regionGroup = mapRegionsRoot.GetComponent<PolygonGlowButtonGroup>();
            if (regionGroup == null)
                regionGroup = mapRegionsRoot.gameObject.AddComponent<PolygonGlowButtonGroup>();
        }

        RebuildWorldButtons();
    }

    private void ResolveDifficultyUiReferences()
    {
        if (difficultyButton == null)
            return;

        if (difficultyButtonLabel != null &&
            difficultyButtonLabel.transform.IsChildOf(difficultyButton.transform))
        {
            return;
        }

        if (difficultyButtonLabel != null)
            difficultyButtonLabel.gameObject.SetActive(false);

        difficultyButtonLabel = difficultyButton.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private void EnsureCampaignReady()
    {
        if (!UseCampaignFlow)
            return;

        CampaignProgressService progress = CampaignProgressService.EnsureExists();
        CampaignRunContext.EnsureExists();
        CampaignRuntimeState.SetCatalog(campaignCatalog);

        if (progress == null)
            return;

        string defaultPlanetId = GetDefaultPlanetId();
        progress.EnsureActiveExpedition(defaultPlanetId);
    }

    private void RegisterLoadingBackgrounds()
    {
        if (worldSelectionCatalog?.entries != null)
        {
            foreach (var entry in worldSelectionCatalog.entries)
                LoadingScreenService.RegisterWorldBackground(entry?.worldConfig);
        }

        if (campaignCatalog?.planets != null)
        {
            foreach (PlanetConfig planet in campaignCatalog.planets)
                LoadingScreenService.RegisterWorldBackground(planet?.worldConfig);
        }
    }

    private void RefreshAll()
    {
        if (UseCampaignFlow && regionViews.Count == 0)
            RebuildWorldButtons();

        RefreshFallbackWorldLabel();
        RefreshDifficultyLabel();
        RefreshPlanetMissionButton();

        if (regionViews.Count == 0)
        {
            ShowNoWorldState();
            return;
        }

        selectedWorldIndex = Mathf.Clamp(selectedWorldIndex, 0, regionViews.Count - 1);
        ApplyWorldSelection(selectedWorldIndex);
    }

    private void RebuildWorldButtons()
    {
        ClearRegionButtons();
        regionViews.Clear();

        if (regionGroup != null)
            regionGroup.buttons.Clear();

        if (UseCampaignFlow)
        {
            List<PlanetConfig> planets = GetAvailablePlanets();
            if (planets.Count == 0)
                return;

            string selectedPlanetId = selectedPlanet != null ? selectedPlanet.planetId : GetActivePlanetId();

            for (int i = 0; i < planets.Count; i++)
            {
                PlanetConfig planet = planets[i];
                WorldSelectionEntry visualEntry = GetVisualEntry(planet, i);
                WorldRegionButtonView view = CreateRegionButton(visualEntry, i);

                regionViews.Add(new RegionRuntime
                {
                    planet = planet,
                    visualEntry = visualEntry,
                    view = view
                });
            }

            int selectedIndex = regionViews.FindIndex(x =>
                x != null &&
                x.planet != null &&
                x.planet.planetId == selectedPlanetId);

            selectedWorldIndex = selectedIndex >= 0 ? selectedIndex : 0;
            return;
        }

        List<WorldSelectionEntry> entries = GetValidEntries();
        if (entries.Count == 0)
            return;

        for (int i = 0; i < entries.Count; i++)
        {
            WorldSelectionEntry entry = entries[i];
            WorldRegionButtonView view = CreateRegionButton(entry, i);

            regionViews.Add(new RegionRuntime
            {
                planet = null,
                visualEntry = entry,
                view = view
            });
        }
    }

    private WorldRegionButtonView CreateRegionButton(WorldSelectionEntry entry, int index)
    {
        if (regionButtonPrefab == null || mapRegionsRoot == null)
            return null;

        WorldRegionButtonView view = Instantiate(regionButtonPrefab, mapRegionsRoot);
        view.name = $"{GetWorldDisplayName(entry, index)}Region";
        view.Bind(entry, () => ApplyWorldSelection(index));

        if (view.Button != null && regionGroup != null)
        {
            regionGroup.buttons.Add(view.Button);
            view.Button.SetGroup(regionGroup);
            view.Button.SetState(ButtonState.Idle);
        }

        return view;
    }

    private void ApplyWorldSelection(int worldIndex)
    {
        if (regionViews.Count == 0)
        {
            ShowNoWorldState();
            return;
        }

        selectedWorldIndex = Mathf.Clamp(worldIndex, 0, regionViews.Count - 1);
        RegionRuntime current = regionViews[selectedWorldIndex];

        if (current.view != null && current.view.Button != null && regionGroup != null)
            regionGroup.OnButtonClicked(current.view.Button);

        if (UseCampaignFlow)
        {
            ApplyCampaignSelection(current);
            return;
        }

        selectedWorldTitle.text = GetWorldDisplayName(current.visualEntry, selectedWorldIndex);
        selectedWorldDescription.text = GetWorldDescription(current.visualEntry);

        List<QuestAsset> quests = GetLegacyQuestPool(current.visualEntry);
        if (selectedQuest == null || !quests.Contains(selectedQuest))
            selectedQuest = quests.FirstOrDefault();

        RebuildQuestButtons(quests);
        RefreshQuestSelection();
        RefreshFallbackWorldLabel();
    }

    private void ApplyCampaignSelection(RegionRuntime current)
    {
        selectedPlanet = current != null ? current.planet : null;

        CampaignProgressService progress = CampaignProgressService.I;
        if (progress != null && selectedPlanet != null)
            progress.SetActivePlanet(selectedPlanet.planetId);

        List<BiomeConfig> biomes = GetSelectedPlanetBiomes();
        selectedBiome = ResolveSelectedBiome(biomes);
        selectedDifficulty = Mathf.Clamp(selectedDifficulty, 1, GetMaxSelectableThreat());

        if (selectedWorldTitle != null)
            selectedWorldTitle.text = GetPlanetDisplayName(selectedPlanet, selectedWorldIndex);

        if (selectedWorldDescription != null)
            selectedWorldDescription.text = GetCampaignDescription();

        List<QuestAsset> quests = GetCampaignQuestPool(current);
        if (selectedQuest == null || !quests.Contains(selectedQuest))
            selectedQuest = quests.FirstOrDefault();

        RebuildQuestButtons(quests);
        RefreshQuestSelection();
        RefreshFallbackWorldLabel();
        RefreshDifficultyLabel();
        RefreshPlanetMissionButton();
    }

    private void CycleBiomeSelection()
    {
        List<BiomeConfig> biomes = GetSelectedPlanetBiomes();
        if (biomes.Count == 0)
            return;

        int currentIndex = FindBiomeIndex(biomes, selectedBiome);
        currentIndex = currentIndex < 0 ? 0 : currentIndex;
        selectedBiome = biomes[(currentIndex + 1) % biomes.Count];

        selectedDifficulty = Mathf.Clamp(selectedDifficulty, 1, GetMaxSelectableThreat());

        RegionRuntime current = GetSelectedRegionRuntime();
        if (selectedWorldDescription != null)
            selectedWorldDescription.text = GetCampaignDescription();

        List<QuestAsset> quests = GetCampaignQuestPool(current);
        if (selectedQuest == null || !quests.Contains(selectedQuest))
            selectedQuest = quests.FirstOrDefault();

        RebuildQuestButtons(quests);
        RefreshQuestSelection();
        RefreshFallbackWorldLabel();
        RefreshDifficultyLabel();
        RefreshPlanetMissionButton();
    }

    private void RebuildQuestButtons(List<QuestAsset> quests)
    {
        ClearQuestButtons();

        if (quests.Count == 0)
        {
            if (selectedQuestTitle != null)
                selectedQuestTitle.text = "No quest selected";

            if (selectedQuestDescription != null)
                selectedQuestDescription.text = "No starting quests are configured for this selection yet.";

            return;
        }

        foreach (QuestAsset quest in quests)
        {
            WorldQuestButtonView view = CreateQuestButton(quest);
            if (view == null)
                continue;

            questViews.Add(new QuestRuntime
            {
                quest = quest,
                view = view
            });
        }
    }

    private WorldQuestButtonView CreateQuestButton(QuestAsset quest)
    {
        if (questButtonPrefab == null || questListRoot == null)
            return null;

        WorldQuestButtonView view = Instantiate(questButtonPrefab, questListRoot);
        view.name = $"{GetQuestDisplayName(quest)}Button";
        view.Bind(quest, () =>
        {
            selectedQuest = quest;
            RefreshQuestSelection();
        });

        return view;
    }

    private void RefreshQuestSelection()
    {
        foreach (QuestRuntime runtime in questViews)
        {
            if (runtime?.view == null)
                continue;

            runtime.view.SetSelected(runtime.quest == selectedQuest);
        }

        if (selectedQuest == null)
        {
            if (selectedQuestTitle != null)
                selectedQuestTitle.text = "No quest selected";

            if (selectedQuestDescription != null)
                selectedQuestDescription.text = "Pick a contract before generating the world.";

            return;
        }

        if (selectedQuestTitle != null)
            selectedQuestTitle.text = GetQuestDisplayName(selectedQuest);

        if (selectedQuestDescription != null)
        {
            selectedQuestDescription.text = string.IsNullOrWhiteSpace(selectedQuest.description)
                ? "This quest does not have a description yet."
                : selectedQuest.description;
        }
    }

    private void RefreshFallbackWorldLabel()
    {
        if (worldConfigButtonLabel == null)
            return;

        if (UseCampaignFlow)
        {
            worldConfigButtonLabel.text = $"Biome: {GetSelectedBiomeName()}";
            return;
        }

        worldConfigButtonLabel.text = $"World: {GetSelectedWorldName()}";
    }

    private void RefreshDifficultyLabel()
    {
        if (difficultyButtonLabel == null)
            return;

        if (UseCampaignFlow)
        {
            difficultyButtonLabel.text = $"Threat: {GetSelectedDifficulty()} / {GetMaxSelectableThreat()}";
            return;
        }

        difficultyButtonLabel.text =
            $"Difficulty: {selectedDifficulty} - {WorldRunBalance.GetDifficultyLabel(selectedDifficulty)}";
    }

    private void RefreshPlanetMissionButton()
    {
        if (launchPlanetMissionButton == null)
            return;

        bool visible = UseCampaignFlow;
        launchPlanetMissionButton.gameObject.SetActive(visible);

        if (!visible)
            return;

        string label = "Launch Planet Mission";
        bool interactable = false;

        PlanetProgressData existingProgress = CampaignProgressService.I != null && selectedPlanet != null
            ? CampaignProgressService.I.GetOrCreatePlanetProgress(selectedPlanet.planetId)
            : null;

        if (existingProgress != null && existingProgress.isPlanetMissionCompleted)
        {
            label = "Planet Mission Completed";

            launchPlanetMissionButton.interactable = false;

            if (launchPlanetMissionButtonLabel != null)
                launchPlanetMissionButtonLabel.text = label;

            return;
        }

        if (!TryGetSelectedPlanetMission(out PlanetConfig planet, out PlanetProgressData progress, out string failureReason))
        {
            label = string.IsNullOrWhiteSpace(failureReason)
                ? "Planet Mission Unavailable"
                : failureReason;
        }
        else if (progress != null && progress.isPlanetMissionCompleted)
        {
            label = "Planet Mission Completed";
        }
        else if (InstanceFinder.NetworkManager == null || !InstanceFinder.NetworkManager.IsServerStarted)
        {
            label = "Host Launches Planet Mission";
        }
        else
        {
            label = $"Launch {GetPlanetDisplayName(planet, selectedWorldIndex)} Mission";
            interactable = !isGeneratingWorld;
        }

        launchPlanetMissionButton.interactable = interactable;

        if (launchPlanetMissionButtonLabel != null)
            launchPlanetMissionButtonLabel.text = label;
    }

    private void ShowNoWorldState()
    {
        if (selectedWorldTitle != null)
            selectedWorldTitle.text = "No world selected";

        if (selectedWorldDescription != null)
        {
            selectedWorldDescription.text = UseCampaignFlow
                ? "No campaign planets are available for the active expedition."
                : "No world entries are configured.";
        }

        if (selectedQuestTitle != null)
            selectedQuestTitle.text = "No quest selected";

        if (selectedQuestDescription != null)
            selectedQuestDescription.text = "No starting quests are configured.";

        ClearQuestButtons();
        RefreshPlanetMissionButton();
    }

    private void ClearRegionButtons()
    {
        if (mapRegionsRoot == null)
            return;

        foreach (Transform child in mapRegionsRoot)
            Destroy(child.gameObject);
    }

    private void ClearQuestButtons()
    {
        foreach (QuestRuntime runtime in questViews)
        {
            if (runtime?.view != null)
                Destroy(runtime.view.gameObject);
        }

        questViews.Clear();
    }

    private List<PlanetConfig> GetAvailablePlanets()
    {
        if (!UseCampaignFlow)
            return new List<PlanetConfig>();

        CampaignProgressService progress = CampaignProgressService.I;
        if (progress == null || progress.ActiveExpedition == null)
            return new List<PlanetConfig>();

        return CampaignCatalogUtility.GetAvailablePlanets(campaignCatalog, progress.ActiveExpedition);
    }

    private List<WorldSelectionEntry> GetValidEntries()
    {
        if (worldSelectionCatalog == null || worldSelectionCatalog.entries == null)
            return new List<WorldSelectionEntry>();

        return worldSelectionCatalog.entries
            .Where(entry => entry != null && entry.worldConfig != null)
            .GroupBy(entry => entry.worldConfig)
            .Select(group => group.First())
            .ToList();
    }

    private RegionRuntime GetSelectedRegionRuntime()
    {
        if (regionViews.Count == 0)
            return null;

        return regionViews[Mathf.Clamp(selectedWorldIndex, 0, regionViews.Count - 1)];
    }

    private WorldSelectionEntry GetSelectedEntry()
    {
        return GetSelectedRegionRuntime()?.visualEntry;
    }

    private List<BiomeConfig> GetSelectedPlanetBiomes()
    {
        return CampaignCatalogUtility.GetPlanetBiomes(selectedPlanet);
    }

    private BiomeConfig ResolveSelectedBiome(List<BiomeConfig> biomes)
    {
        if (biomes == null || biomes.Count == 0)
            return null;

        int index = FindBiomeIndex(biomes, selectedBiome);
        if (index >= 0)
            return biomes[index];

        return biomes[0];
    }

    private int FindBiomeIndex(List<BiomeConfig> biomes, BiomeConfig biome)
    {
        if (biome == null || biomes == null)
            return -1;

        for (int i = 0; i < biomes.Count; i++)
        {
            if (biomes[i] == biome)
                return i;

            if (!string.IsNullOrWhiteSpace(biome.biomeID) &&
                biomes[i] != null &&
                biomes[i].biomeID == biome.biomeID)
            {
                return i;
            }
        }

        return -1;
    }

    private List<QuestAsset> GetCampaignQuestPool(RegionRuntime runtime)
    {
        List<QuestAsset> biomeQuests = CampaignCatalogUtility.GetQuestPoolFromBiome(selectedBiome)
            .Where(IsValidQuest)
            .Distinct()
            .ToList();

        if (biomeQuests.Count > 0)
            return biomeQuests;

        return GetLegacyQuestPool(runtime != null ? runtime.visualEntry : null);
    }

    private List<QuestAsset> GetLegacyQuestPool(WorldSelectionEntry entry)
    {
        if (entry == null || entry.availableQuests == null)
            return new List<QuestAsset>();

        return entry.availableQuests
            .Where(IsValidQuest)
            .Distinct()
            .ToList();
    }

    private List<string> GetSelectedQuestIds()
    {
        if (!IsValidQuest(selectedQuest))
            return new List<string>();

        return new List<string> { selectedQuest.questId };
    }

    private List<string> GetSelectedChainIds()
    {
        return new List<string>();
    }

    private string GetSelectedWorldConfigId()
    {
        if (UseCampaignFlow)
            return CampaignCatalogUtility.GetWorldConfigId(selectedPlanet);

        WorldSelectionEntry entry = GetSelectedEntry();
        if (entry == null || entry.worldConfig == null)
            return string.Empty;

        return entry.worldConfig.name;
    }

    private WorldConfig GetSelectedWorldConfig()
    {
        if (UseCampaignFlow)
            return selectedPlanet != null ? selectedPlanet.worldConfig : null;

        return GetSelectedEntry()?.worldConfig;
    }

    private string GetSelectedWorldName()
    {
        if (UseCampaignFlow)
            return GetPlanetDisplayName(selectedPlanet, selectedWorldIndex);

        WorldSelectionEntry entry = GetSelectedEntry();
        return entry != null ? GetWorldDisplayName(entry, selectedWorldIndex) : "Default";
    }

    private int GetSelectedDifficulty()
    {
        if (UseCampaignFlow)
            return Mathf.Max(1, selectedDifficulty);

        return WorldRunBalance.ClampDifficulty(selectedDifficulty);
    }

    private int GetMaxSelectableThreat()
    {
        if (!UseCampaignFlow)
            return 1;

        return CampaignCatalogUtility.GetMaxSelectableThreat(
            campaignCatalog,
            CampaignProgressService.I,
            selectedPlanet,
            selectedBiome);
    }

    private int GetShipThreatCap()
    {
        if (!UseCampaignFlow || CampaignProgressService.I == null)
            return 1;

        return CampaignCatalogUtility.GetShipThreatCap(campaignCatalog, CampaignProgressService.I.ShipLevel);
    }

    private string GetWorldDisplayName(WorldSelectionEntry entry, int index)
    {
        if (entry == null)
            return $"World {index + 1}";

        if (!string.IsNullOrWhiteSpace(entry.displayName))
            return entry.displayName;

        if (entry.worldConfig != null && !string.IsNullOrWhiteSpace(entry.worldConfig.name))
            return entry.worldConfig.name;

        return $"World {index + 1}";
    }

    private string GetWorldDescription(WorldSelectionEntry entry)
    {
        if (entry == null)
            return "New runtime world preset.";

        return string.IsNullOrWhiteSpace(entry.description)
            ? "New runtime world preset."
            : entry.description;
    }

    private string GetPlanetDisplayName(PlanetConfig planet, int index)
    {
        if (planet == null)
            return $"Planet {index + 1}";

        if (!string.IsNullOrWhiteSpace(planet.displayName))
            return planet.displayName;

        if (planet.worldConfig != null && !string.IsNullOrWhiteSpace(planet.worldConfig.name))
            return planet.worldConfig.name;

        return !string.IsNullOrWhiteSpace(planet.planetId)
            ? planet.planetId
            : $"Planet {index + 1}";
    }

    private string GetCampaignDescription()
    {
        if (selectedPlanet == null)
            return "No planet selected.";

        string description = string.IsNullOrWhiteSpace(selectedPlanet.description)
            ? "No planet description yet."
            : selectedPlanet.description;

        string biomeLine = selectedBiome != null
            ? $"Biome: {GetSelectedBiomeName()}."
            : "Biome: not configured.";

        int unlockedThreat = selectedPlanet != null && selectedBiome != null && CampaignProgressService.I != null
            ? CampaignProgressService.I.GetMaxUnlockedThreat(selectedPlanet.planetId, selectedBiome.biomeID)
            : 1;

        int maxThreat = GetMaxSelectableThreat();
        string planetMissionLine = GetPlanetMissionProgressText(selectedPlanet, selectedBiome);
        return $"{description}\n{biomeLine}\nUnlocked Threat: {Mathf.Min(unlockedThreat, maxThreat)} / {maxThreat}\n{planetMissionLine}";
    }

    private string GetSelectedBiomeName()
    {
        if (selectedBiome == null)
            return "None";

        if (!string.IsNullOrWhiteSpace(selectedBiome.biomeName))
            return selectedBiome.biomeName;

        if (!string.IsNullOrWhiteSpace(selectedBiome.name))
            return selectedBiome.name;

        return selectedBiome.biomeID;
    }

    private string GetQuestDisplayName(QuestAsset quest)
    {
        if (quest == null)
            return "Unknown quest";

        if (!string.IsNullOrWhiteSpace(quest.questName))
            return quest.questName;

        if (!string.IsNullOrWhiteSpace(quest.name))
            return quest.name;

        return quest.questId;
    }

    private bool IsValidQuest(QuestAsset quest)
    {
        return quest != null && !string.IsNullOrWhiteSpace(quest.questId);
    }

    private bool IsValidChain(QuestChainAsset chain)
    {
        return chain != null && !string.IsNullOrWhiteSpace(chain.chainId);
    }

    private void ResetSubmissionState()
    {
        isGeneratingWorld = false;

        if (generateWorldButton != null)
            generateWorldButton.interactable = true;

        RefreshPlanetMissionButton();
    }

    private string GetDefaultPlanetId()
    {
        if (campaignCatalog == null || campaignCatalog.planets == null)
            return string.Empty;

        PlanetConfig planet = campaignCatalog.planets.FirstOrDefault(x =>
            x != null &&
            !string.IsNullOrWhiteSpace(x.planetId) &&
            x.worldConfig != null);

        return planet != null ? planet.planetId : string.Empty;
    }

    private string GetActivePlanetId()
    {
        return CampaignProgressService.I?.ActiveExpedition != null
            ? CampaignProgressService.I.ActiveExpedition.activePlanetId
            : string.Empty;
    }

    private WorldSelectionEntry GetVisualEntry(PlanetConfig planet, int index)
    {
        WorldSelectionEntry existing = FindLegacyEntry(planet);

        return new WorldSelectionEntry
        {
            worldConfig = planet != null ? planet.worldConfig : null,
            displayName = GetPlanetDisplayName(planet, index),
            description = planet != null ? planet.description : "Campaign planet.",
            position = GetCampaignPlanetButtonPosition(index),
            size = campaignPlanetButtonSize,
            rotation = 0f,
            regionSprite = existing != null ? existing.regionSprite : null,
            idleColor = existing != null
                ? existing.idleColor
                : new Color(0.25f, 0.55f, 0.58f, 0.92f),
            selectedColor = existing != null
                ? existing.selectedColor
                : new Color(0.45f, 0.8f, 0.84f, 0.98f),
            lockedColor = new Color(0f, 0f, 0f, 0f),
            availableQuests = new QuestAsset[0],
            availableChains = new QuestChainAsset[0]
        };
    }

    private WorldSelectionEntry FindLegacyEntry(PlanetConfig planet)
    {
        if (planet == null || planet.worldConfig == null || worldSelectionCatalog?.entries == null)
            return null;

        return worldSelectionCatalog.entries.FirstOrDefault(x =>
            x != null &&
            x.worldConfig == planet.worldConfig);
    }

    private Vector2 GetCampaignPlanetButtonPosition(int index)
    {
        return new Vector2(
            campaignPlanetButtonX,
            campaignPlanetButtonStartY - (campaignPlanetButtonSpacing * index));
    }

    private bool TryGetSelectedPlanetMission(
        out PlanetConfig planet,
        out PlanetProgressData progress,
        out string failureReason)
    {
        planet = selectedPlanet;
        progress = null;
        failureReason = string.Empty;

        if (!UseCampaignFlow)
        {
            failureReason = "Planet mission is available only in campaign mode.";
            return false;
        }

        if (planet == null)
        {
            failureReason = "Select a planet first.";
            return false;
        }

        CampaignProgressService campaignProgress = CampaignProgressService.I;
        if (campaignProgress == null || campaignProgress.ActiveExpedition == null)
        {
            failureReason = "Active expedition is not selected.";
            return false;
        }

        campaignProgress.TryUnlockPlanetMission(planet, selectedBiome != null ? selectedBiome.biomeID : string.Empty);

        progress = campaignProgress.GetOrCreatePlanetProgress(planet.planetId);
        if (progress == null)
        {
            failureReason = "Planet progress is not available.";
            return false;
        }

        if (!progress.isPlanetMissionUnlocked)
        {
            failureReason = GetPlanetMissionLockedLabel(planet, selectedBiome);
            return false;
        }

        if (string.IsNullOrWhiteSpace(planet.planetMissionSceneName))
        {
            failureReason = "Planet mission scene is not assigned.";
            return false;
        }

        return true;
    }

    private string GetPlanetMissionProgressText(PlanetConfig planet, BiomeConfig biome)
    {
        if (planet == null)
            return "Planet Mission: unavailable.";

        if (biome == null)
            return $"Planet Mission: select region and reach Threat {planet.planetMissionUnlockThreatLevel}.";

        CampaignProgressService campaignProgress = CampaignProgressService.I;
        if (campaignProgress == null || campaignProgress.ActiveExpedition == null)
            return $"Planet Mission: region needs Threat {planet.planetMissionUnlockThreatLevel}.";

        BiomeThreatProgressData biomeProgress = campaignProgress.GetOrCreateBiomeProgress(
            planet.planetId,
            biome.biomeID);

        bool unlocked = biomeProgress != null &&
                        biomeProgress.completedThreatLevels != null &&
                        biomeProgress.completedThreatLevels.Contains(planet.planetMissionUnlockThreatLevel);

        PlanetProgressData planetProgress = campaignProgress.GetOrCreatePlanetProgress(planet.planetId);
        if (planetProgress != null && planetProgress.isPlanetMissionCompleted)
            return "Planet Mission: completed.";

        if (planetProgress != null && planetProgress.isPlanetMissionUnlocked)
            return $"Planet Mission: ready for region {GetSelectedBiomeName()}.";

        int currentThreat = biomeProgress != null ? biomeProgress.maxUnlockedThreatLevel : 1;
        return $"Planet Mission: region {GetSelectedBiomeName()} needs Threat {planet.planetMissionUnlockThreatLevel} (current {Mathf.Max(1, currentThreat)}).";
    }

    private string GetPlanetMissionLockedLabel(PlanetConfig planet, BiomeConfig biome)
    {
        if (planet == null)
            return "Planet Mission Locked";

        if (biome == null)
            return "Select Region First";

        CampaignProgressService campaignProgress = CampaignProgressService.I;
        if (campaignProgress == null || campaignProgress.ActiveExpedition == null)
            return $"Locked (Threat {planet.planetMissionUnlockThreatLevel})";

        BiomeThreatProgressData biomeProgress = campaignProgress.GetOrCreateBiomeProgress(
            planet.planetId,
            biome.biomeID);

        int currentThreat = biomeProgress != null
            ? Mathf.Max(1, biomeProgress.maxUnlockedThreatLevel)
            : 1;

        return $"Locked (Region Threat {currentThreat}/{planet.planetMissionUnlockThreatLevel})";
    }
}
