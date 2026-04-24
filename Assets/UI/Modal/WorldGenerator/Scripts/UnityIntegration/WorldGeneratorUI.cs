using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.Input;
using Features.UI;
using Features.Quests.Data;

public sealed class WorldGeneratorUI : PlayerBoundUIView, IUIScreen
{
    private sealed class RegionRuntime
    {
        public WorldSelectionEntry entry;
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

    [Header("Fallback Selector")]
    [SerializeField] private Button worldConfigButton;
    [SerializeField] private TextMeshProUGUI worldConfigButtonLabel;

    public InputMode Mode => InputMode.Dialog;

    private readonly List<RegionRuntime> regionViews = new();
    private readonly List<QuestRuntime> questViews = new();

    private PolygonGlowButtonGroup regionGroup;
    private int selectedWorldIndex;
    private QuestAsset selectedQuest;
    private bool initialized;
    private bool isGeneratingWorld;

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

        isGeneratingWorld = true;

        if (generateWorldButton != null)
            generateWorldButton.interactable = false;

        WorldSelectionEntry selectedEntry = GetSelectedEntry();
        LoadingScreenService.ShowWorld(
            selectedEntry != null ? selectedEntry.worldConfig : null,
            "Generating procedural world...");

        string worldConfigId = GetSelectedWorldConfigId();
        List<string> questIds = GetSelectedQuestIds();
        List<string> chainIds = GetSelectedChainIds();

        var net = BoundPlayer.GetComponent<PlayerSessionNetwork>();
        net.RequestWorldServerRpc(worldConfigId, questIds, chainIds);

        UIStackManager.I?.Clear();
    }

    public void OnCycleWorldConfigClicked()
    {
        if (regionViews.Count == 0)
            return;

        selectedWorldIndex = (selectedWorldIndex + 1) % regionViews.Count;
        RefreshAll();
    }

    private void Initialize()
    {
        if (initialized)
            return;

        initialized = true;
        RegisterLoadingBackgrounds();

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

        if (mapRegionsRoot != null)
        {
            regionGroup = mapRegionsRoot.GetComponent<PolygonGlowButtonGroup>();
            if (regionGroup == null)
                regionGroup = mapRegionsRoot.gameObject.AddComponent<PolygonGlowButtonGroup>();
        }

        RebuildWorldButtons();
    }

    private void RegisterLoadingBackgrounds()
    {
        if (worldSelectionCatalog?.entries == null)
            return;

        foreach (var entry in worldSelectionCatalog.entries)
            LoadingScreenService.RegisterWorldBackground(entry?.worldConfig);
    }

    private void RefreshAll()
    {
        RefreshFallbackWorldLabel();

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

        List<WorldSelectionEntry> entries = GetValidEntries();
        if (entries.Count == 0)
            return;

        if (regionGroup != null)
            regionGroup.buttons.Clear();

        for (int i = 0; i < entries.Count; i++)
        {
            WorldSelectionEntry entry = entries[i];
            WorldRegionButtonView view = CreateRegionButton(entry, i);

            regionViews.Add(new RegionRuntime
            {
                entry = entry,
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

        selectedWorldTitle.text = GetWorldDisplayName(current.entry, selectedWorldIndex);
        selectedWorldDescription.text = GetWorldDescription(current.entry);

        List<QuestAsset> quests = GetQuestPool(current.entry);
        if (selectedQuest == null || !quests.Contains(selectedQuest))
            selectedQuest = quests.FirstOrDefault();

        RebuildQuestButtons(quests);
        RefreshQuestSelection();
        RefreshFallbackWorldLabel();
    }

    private void RebuildQuestButtons(List<QuestAsset> quests)
    {
        ClearQuestButtons();

        if (quests.Count == 0)
        {
            selectedQuestTitle.text = "No quest selected";
            selectedQuestDescription.text = "No starting quests are configured for this world yet.";
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
            selectedQuestTitle.text = "No quest selected";
            selectedQuestDescription.text = "Pick a contract before generating the world.";
            return;
        }

        selectedQuestTitle.text = GetQuestDisplayName(selectedQuest);
        selectedQuestDescription.text = string.IsNullOrWhiteSpace(selectedQuest.description)
            ? "This quest does not have a description yet."
            : selectedQuest.description;
    }

    private void RefreshFallbackWorldLabel()
    {
        if (worldConfigButtonLabel == null)
            return;

        worldConfigButtonLabel.text = $"World: {GetSelectedWorldName()}";
    }

    private void ShowNoWorldState()
    {
        if (selectedWorldTitle != null)
            selectedWorldTitle.text = "No world selected";

        if (selectedWorldDescription != null)
            selectedWorldDescription.text = "No world entries are configured.";

        if (selectedQuestTitle != null)
            selectedQuestTitle.text = "No quest selected";

        if (selectedQuestDescription != null)
            selectedQuestDescription.text = "No starting quests are configured.";

        ClearQuestButtons();
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

    private WorldSelectionEntry GetSelectedEntry()
    {
        if (regionViews.Count == 0)
            return null;

        return regionViews[Mathf.Clamp(selectedWorldIndex, 0, regionViews.Count - 1)].entry;
    }

    private List<QuestAsset> GetQuestPool(WorldSelectionEntry entry)
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
        // World selection currently has no explicit chain picker, so auto-starting
        // every chain from the selected region creates hidden extra quests.
        return new List<string>();
    }

    private string GetSelectedWorldConfigId()
    {
        WorldSelectionEntry entry = GetSelectedEntry();
        if (entry == null || entry.worldConfig == null)
            return string.Empty;

        return entry.worldConfig.name;
    }

    private string GetSelectedWorldName()
    {
        WorldSelectionEntry entry = GetSelectedEntry();
        return entry != null ? GetWorldDisplayName(entry, selectedWorldIndex) : "Default";
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
    }
}
