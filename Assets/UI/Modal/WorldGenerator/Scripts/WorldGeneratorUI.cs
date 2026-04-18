using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Features.Input;
using Features.UI;
using System.Collections.Generic;
using System.Linq;
using Features.Quests.Data;
using Biomes.Data;

public sealed class WorldGeneratorUI : PlayerBoundUIView, IUIScreen
{
    private enum RegionShape
    {
        Desert,
        Cave,
        Mountains
    }

    private struct RegionLayout
    {
        public string displayName;
        public string description;
        public Vector2 position;
        public Vector2 size;
        public float rotation;
        public RegionShape shape;
        public Color idleColor;
        public Color selectedColor;
    }

    private sealed class RegionRuntime
    {
        public WorldConfig worldConfig;
        public PolygonGlowButton button;
        public string description;
        public string displayName;
    }

    [SerializeField] private GameObject root;
    [SerializeField] private TMP_InputField seedInput;
    [SerializeField] private Button randomSeedButton;
    [SerializeField] private Button generateWorldButton;
    [SerializeField] private GameObject polygonButtonTemplate;
    [SerializeField] private Sprite mapBackgroundSprite;
    [SerializeField] private Sprite desertRegionSprite;
    [SerializeField] private Sprite caveRegionSprite;
    [SerializeField] private Sprite mountainsRegionSprite;
    [SerializeField] private WorldConfig[] availableWorldConfigs;
    [SerializeField] private Button worldConfigButton;
    [SerializeField] private TextMeshProUGUI worldConfigButtonLabel;

    [Header("Available quests")]
    [SerializeField] private QuestAsset[] availableQuests;

    [Header("Available chains")]
    [SerializeField] private QuestChainAsset[] availableChains;

    public InputMode Mode => InputMode.Dialog;

    private int selectedWorldConfigIndex;
    private bool worldConfigSelectorInitialized;
    private QuestAsset selectedQuest;

    private RectTransform mapShell;
    private RectTransform mapRegionsRoot;
    private RectTransform questListRoot;
    private PolygonGlowButtonGroup regionGroup;
    private TextMeshProUGUI selectedWorldTitle;
    private TextMeshProUGUI selectedWorldDescription;
    private TextMeshProUGUI selectedQuestTitle;
    private TextMeshProUGUI selectedQuestDescription;

    private readonly List<RegionRuntime> regionViews = new();
    private readonly List<Button> questButtons = new();

    protected override void OnEnable()
    {
        base.OnEnable();
        UIRegistry.I?.Register(this);
        EnsureWorldConfigSelector();
        root.SetActive(false);
    }

    protected override void OnDisable()
    {
        UIRegistry.I?.Unregister(this);
        base.OnDisable();
    }

    protected override void OnPlayerBound(GameObject player)
    {
        root.SetActive(false);
    }

    public void Show()
    {
        EnsureWorldConfigSelector();
        HideSeedControls();
        RefreshSelectionUi();
        root.SetActive(true);
        InputModeManager.I.SetMode(Mode);
    }

    public void Hide()
    {
        root.SetActive(false);
        InputModeManager.I.SetMode(InputMode.Gameplay);
    }

    public void Open()
    {
        UIStackManager.I.Push(this);
    }

    public void OnRandomSeedClicked()
    {
        Debug.Log("[WorldGeneratorUI] Seed is generated automatically on the server.");
    }

    public void OnGenerateWorldClicked()
    {
        if (BoundPlayer == null)
        {
            Debug.LogError("WorldGeneratorUI: BoundPlayer is null");
            return;
        }

        var questIds = GetSelectedQuestIds();
        var chainIds = GetSelectedChainIds();

        var net = BoundPlayer.GetComponent<PlayerSessionNetwork>();
        string worldConfigId = GetSelectedWorldConfigId();

        net.RequestWorldServerRpc(worldConfigId, questIds, chainIds);

        UIStackManager.I?.Clear();
    }

    public void OnCloseClicked()
    {
        UIStackManager.I.Pop();
    }

    public void OnCycleWorldConfigClicked()
    {
        if (availableWorldConfigs == null || availableWorldConfigs.Length == 0)
            return;

        selectedWorldConfigIndex = (selectedWorldConfigIndex + 1) % availableWorldConfigs.Length;
        RefreshSelectionUi();
    }

    private void EnsureWorldConfigSelector()
    {
        if (worldConfigSelectorInitialized)
            return;

        worldConfigSelectorInitialized = true;
        TryAutoBindPrimaryButtons();
        HideSeedControls();

        availableWorldConfigs = availableWorldConfigs == null
            ? null
            : availableWorldConfigs
                .Where(cfg => cfg != null)
                .Distinct()
                .ToArray();

        availableQuests = availableQuests == null
            ? null
            : availableQuests
                .Where(IsValidQuest)
                .Distinct()
                .ToArray();

        availableChains = availableChains == null
            ? null
            : availableChains
                .Where(IsValidChain)
                .Distinct()
                .ToArray();

        if (availableWorldConfigs == null || availableWorldConfigs.Length == 0 || root == null)
            return;

        selectedWorldConfigIndex = Mathf.Clamp(selectedWorldConfigIndex, 0, availableWorldConfigs.Length - 1);

        if (CanBuildMapUi())
        {
            BuildMapUi();
        }
        else
        {
            if (worldConfigButton == null)
                CreateRuntimeWorldConfigButton();

            if (worldConfigButton != null)
            {
                worldConfigButton.onClick.RemoveListener(OnCycleWorldConfigClicked);
                worldConfigButton.onClick.AddListener(OnCycleWorldConfigClicked);
            }
        }

        RefreshSelectionUi();
    }

    private void CreateRuntimeWorldConfigButton()
    {
        var buttonGO = new GameObject(
            "WorldConfigButton",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));

        buttonGO.transform.SetParent(root.transform, false);

        var rect = (RectTransform)buttonGO.transform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(320f, 34f);

        var seedRect = seedInput != null ? seedInput.transform as RectTransform : null;
        rect.anchoredPosition = seedRect != null
            ? seedRect.anchoredPosition + new Vector2(0f, -48f)
            : new Vector2(0f, 180f);

        var image = buttonGO.GetComponent<Image>();
        worldConfigButton = buttonGO.GetComponent<Button>();

        var templateButton = root.GetComponentsInChildren<Button>(true)
            .FirstOrDefault(btn => btn != null && btn != worldConfigButton);

        if (templateButton != null)
        {
            var templateImage = templateButton.targetGraphic as Image;
            if (templateImage != null)
            {
                image.sprite = templateImage.sprite;
                image.type = templateImage.type;
                image.material = templateImage.material;
                image.color = templateImage.color;
            }

            worldConfigButton.transition = templateButton.transition;
            worldConfigButton.colors = templateButton.colors;
        }
        else
        {
            image.color = Color.white;
        }

        worldConfigButton.targetGraphic = image;

        var labelGO = new GameObject(
            "Label",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));

        labelGO.transform.SetParent(buttonGO.transform, false);

        var labelRect = (RectTransform)labelGO.transform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        worldConfigButtonLabel = labelGO.GetComponent<TextMeshProUGUI>();
        worldConfigButtonLabel.alignment = TextAlignmentOptions.Center;
        worldConfigButtonLabel.textWrappingMode = TextWrappingModes.NoWrap;

        if (seedInput != null && seedInput.textComponent != null)
        {
            worldConfigButtonLabel.font = seedInput.textComponent.font;
            worldConfigButtonLabel.fontSize = seedInput.textComponent.fontSize;
            worldConfigButtonLabel.color = seedInput.textComponent.color;
        }
        else
        {
            worldConfigButtonLabel.fontSize = 24f;
            worldConfigButtonLabel.color = Color.black;
        }
    }

    private void RefreshWorldConfigLabel()
    {
        if (worldConfigButtonLabel == null)
            return;

        worldConfigButtonLabel.text = $"World: {GetSelectedWorldConfigName()}";
    }

    private void RefreshSelectionUi()
    {
        RefreshWorldConfigLabel();

        if (regionViews.Count == 0)
            return;

        selectedWorldConfigIndex = Mathf.Clamp(selectedWorldConfigIndex, 0, regionViews.Count - 1);
        ApplyWorldSelection(selectedWorldConfigIndex, true);
    }

    private string GetSelectedWorldConfigId()
    {
        if (availableWorldConfigs == null || availableWorldConfigs.Length == 0)
            return string.Empty;

        var config = availableWorldConfigs[Mathf.Clamp(selectedWorldConfigIndex, 0, availableWorldConfigs.Length - 1)];
        return config != null ? config.name : string.Empty;
    }

    private string GetSelectedWorldConfigName()
    {
        if (availableWorldConfigs == null || availableWorldConfigs.Length == 0)
            return "Default";

        var config = availableWorldConfigs[Mathf.Clamp(selectedWorldConfigIndex, 0, availableWorldConfigs.Length - 1)];
        return config != null ? config.name : "Default";
    }

    private bool CanBuildMapUi()
    {
        return polygonButtonTemplate != null
            && mapBackgroundSprite != null
            && desertRegionSprite != null
            && caveRegionSprite != null
            && mountainsRegionSprite != null;
    }

    private void BuildMapUi()
    {
        if (mapShell != null)
            return;

        if (worldConfigButton != null)
            worldConfigButton.gameObject.SetActive(false);

        RepositionPrimaryControls();

        mapShell = CreateStretchRect("WorldMapShell", root.transform as RectTransform);
        mapShell.anchorMin = new Vector2(0.07f, 0.14f);
        mapShell.anchorMax = new Vector2(0.93f, 0.74f);
        mapShell.offsetMin = Vector2.zero;
        mapShell.offsetMax = Vector2.zero;

        var shellImage = mapShell.gameObject.AddComponent<Image>();
        shellImage.sprite = mapBackgroundSprite;
        shellImage.type = Image.Type.Simple;
        shellImage.color = new Color(1f, 1f, 1f, 0.98f);

        mapRegionsRoot = CreateStretchRect("Regions", mapShell);
        mapRegionsRoot.anchorMin = new Vector2(0.03f, 0.06f);
        mapRegionsRoot.anchorMax = new Vector2(0.68f, 0.94f);
        mapRegionsRoot.offsetMin = Vector2.zero;
        mapRegionsRoot.offsetMax = Vector2.zero;

        regionGroup = mapRegionsRoot.gameObject.AddComponent<PolygonGlowButtonGroup>();

        var detailsPanel = CreateStretchRect("DetailsPanel", mapShell);
        detailsPanel.anchorMin = new Vector2(0.72f, 0.08f);
        detailsPanel.anchorMax = new Vector2(0.97f, 0.92f);
        detailsPanel.offsetMin = Vector2.zero;
        detailsPanel.offsetMax = Vector2.zero;

        var detailsImage = detailsPanel.gameObject.AddComponent<Image>();
        detailsImage.color = new Color(0.08f, 0.11f, 0.13f, 0.92f);

        selectedWorldTitle = CreateText("SelectedWorldTitle", detailsPanel, 28, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        Stretch(selectedWorldTitle.rectTransform, new Vector2(0.06f, 0.82f), new Vector2(0.94f, 0.96f));

        selectedWorldDescription = CreateText("SelectedWorldDescription", detailsPanel, 18, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        selectedWorldDescription.color = new Color(0.85f, 0.92f, 0.95f, 0.95f);
        Stretch(selectedWorldDescription.rectTransform, new Vector2(0.06f, 0.62f), new Vector2(0.94f, 0.82f));

        var questListLabel = CreateText("QuestListLabel", detailsPanel, 20, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        questListLabel.text = "Quest Contract";
        Stretch(questListLabel.rectTransform, new Vector2(0.06f, 0.54f), new Vector2(0.94f, 0.62f));

        questListRoot = CreateStretchRect("QuestList", detailsPanel);
        questListRoot.anchorMin = new Vector2(0.06f, 0.24f);
        questListRoot.anchorMax = new Vector2(0.94f, 0.54f);
        questListRoot.offsetMin = Vector2.zero;
        questListRoot.offsetMax = Vector2.zero;

        var questLayout = questListRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        questLayout.spacing = 10f;
        questLayout.childControlWidth = true;
        questLayout.childControlHeight = false;
        questLayout.childForceExpandWidth = true;
        questLayout.childForceExpandHeight = false;
        questLayout.padding = new RectOffset(0, 0, 0, 0);

        var questFitter = questListRoot.gameObject.AddComponent<ContentSizeFitter>();
        questFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        questFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        selectedQuestTitle = CreateText("SelectedQuestTitle", detailsPanel, 20, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        Stretch(selectedQuestTitle.rectTransform, new Vector2(0.06f, 0.16f), new Vector2(0.94f, 0.24f));

        selectedQuestDescription = CreateText("SelectedQuestDescription", detailsPanel, 16, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        selectedQuestDescription.color = new Color(0.83f, 0.88f, 0.9f, 0.92f);
        Stretch(selectedQuestDescription.rectTransform, new Vector2(0.06f, 0.04f), new Vector2(0.94f, 0.16f));

        BuildRegionButtons();
    }

    private void BuildRegionButtons()
    {
        regionViews.Clear();
        regionGroup.buttons.Clear();

        for (int i = 0; i < availableWorldConfigs.Length; i++)
        {
            WorldConfig config = availableWorldConfigs[i];
            RegionLayout layout = GetRegionLayout(config, i);
            PolygonGlowButton button = CreateRegionButton(config, layout, i);

            regionViews.Add(new RegionRuntime
            {
                worldConfig = config,
                button = button,
                description = layout.description,
                displayName = layout.displayName
            });
        }
    }

    private PolygonGlowButton CreateRegionButton(WorldConfig config, RegionLayout layout, int index)
    {
        var instance = Instantiate(polygonButtonTemplate, mapRegionsRoot);
        instance.name = $"{layout.displayName}Region";

        var rect = instance.transform as RectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = layout.position;
        rect.sizeDelta = layout.size;
        rect.localEulerAngles = new Vector3(0f, 0f, layout.rotation);

        var button = instance.GetComponent<PolygonGlowButton>();
        if (button == null)
            button = instance.AddComponent<PolygonGlowButton>();

        Sprite shapeSprite = GetShapeSprite(layout.shape);

        button.baseImage.sprite = shapeSprite;
        button.baseImage.useSpriteMesh = true;
        button.baseImage.rectTransform.sizeDelta = layout.size;
        button.baseImage.rectTransform.anchoredPosition = Vector2.zero;

        button.glowImage.sprite = shapeSprite;
        button.glowImage.useSpriteMesh = true;
        button.glowImage.rectTransform.sizeDelta = layout.size;
        button.glowImage.rectTransform.anchoredPosition = Vector2.zero;

        button.idleColor = layout.idleColor;
        button.selectedColor = layout.selectedColor;
        button.lockedColor = new Color(layout.idleColor.r * 0.35f, layout.idleColor.g * 0.35f, layout.idleColor.b * 0.35f, 0.18f);
        button.hoverHighlight = 1.15f;
        button.selectedHighlight = 0.6f;
        button.fadeSpeed = 8f;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => ApplyWorldSelection(index, false));

        regionGroup.buttons.Add(button);
        button.SetGroup(regionGroup);
        button.SetState(ButtonState.Idle);

        var label = CreateText("Label", rect, 20, FontStyles.Bold, TextAlignmentOptions.Center);
        label.text = FormatRegionLabel(layout.displayName);
        label.color = new Color(0.96f, 0.98f, 0.99f, 0.96f);
        label.raycastTarget = false;
        Stretch(label.rectTransform, new Vector2(0.15f, 0.2f), new Vector2(0.85f, 0.8f));

        return button;
    }

    private void ApplyWorldSelection(int worldIndex, bool forceRefresh)
    {
        if (regionViews.Count == 0)
            return;

        selectedWorldConfigIndex = Mathf.Clamp(worldIndex, 0, regionViews.Count - 1);
        RegionRuntime region = regionViews[selectedWorldConfigIndex];

        if (forceRefresh || region.button != null)
            regionGroup.OnButtonClicked(region.button);

        selectedWorldTitle.text = region.displayName;
        selectedWorldDescription.text = region.description;

        var questPool = GetQuestPoolFor(region.worldConfig);
        if (selectedQuest == null || !questPool.Contains(selectedQuest))
            selectedQuest = questPool.FirstOrDefault();

        RebuildQuestButtons(questPool);
        RefreshQuestSelection();
    }

    private void RebuildQuestButtons(List<QuestAsset> quests)
    {
        foreach (var button in questButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }

        questButtons.Clear();

        if (quests.Count == 0)
        {
            selectedQuestTitle.text = "No quest selected";
            selectedQuestDescription.text = "No starting quests are configured for this world yet.";
            return;
        }

        foreach (QuestAsset quest in quests)
        {
            var button = CreateQuestButton(questListRoot, quest);
            questButtons.Add(button);
        }
    }

    private Button CreateQuestButton(Transform parent, QuestAsset quest)
    {
        var buttonGO = new GameObject(
            $"{GetQuestDisplayName(quest)}Button",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button),
            typeof(LayoutElement));

        buttonGO.transform.SetParent(parent, false);

        var rect = (RectTransform)buttonGO.transform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(0f, 44f);

        var layout = buttonGO.GetComponent<LayoutElement>();
        layout.minHeight = 44f;
        layout.preferredHeight = 44f;

        var image = buttonGO.GetComponent<Image>();
        image.color = new Color(0.15f, 0.19f, 0.22f, 0.96f);

        var button = buttonGO.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = image;
        button.onClick.AddListener(() =>
        {
            selectedQuest = quest;
            RefreshQuestSelection();
        });

        var label = CreateText("Label", rect, 18, FontStyles.Bold, TextAlignmentOptions.Center);
        label.text = GetQuestDisplayName(quest);
        label.raycastTarget = false;
        Stretch(label.rectTransform, Vector2.zero, Vector2.one);

        return button;
    }

    private void RefreshQuestSelection()
    {
        if (questButtons.Count > 0)
        {
            for (int i = 0; i < questButtons.Count; i++)
            {
                if (questButtons[i] == null)
                    continue;

                var image = questButtons[i].targetGraphic as Image;
                if (image == null)
                    continue;

                QuestAsset quest = GetQuestPoolForSelectedWorld().ElementAtOrDefault(i);
                bool isSelected = quest == selectedQuest;
                image.color = isSelected
                    ? new Color(0.27f, 0.43f, 0.48f, 0.98f)
                    : new Color(0.15f, 0.19f, 0.22f, 0.96f);
            }
        }

        if (selectedQuest == null)
        {
            selectedQuestTitle.text = "No quest selected";
            selectedQuestDescription.text = "Pick a contract in the right panel before generating the world.";
            return;
        }

        selectedQuestTitle.text = GetQuestDisplayName(selectedQuest);
        selectedQuestDescription.text = string.IsNullOrWhiteSpace(selectedQuest.description)
            ? "This quest does not have a description yet."
            : selectedQuest.description;
    }

    private List<QuestAsset> GetQuestPoolForSelectedWorld()
    {
        if (availableWorldConfigs == null || availableWorldConfigs.Length == 0)
            return new List<QuestAsset>();

        WorldConfig config = availableWorldConfigs[Mathf.Clamp(selectedWorldConfigIndex, 0, availableWorldConfigs.Length - 1)];
        return GetQuestPoolFor(config);
    }

    private List<QuestAsset> GetQuestPoolFor(WorldConfig worldConfig)
    {
        if (availableQuests == null)
            return new List<QuestAsset>();

        // For now every valid starting quest can be launched from any region.
        return availableQuests
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
        if (availableChains == null)
            return new List<string>();

        return availableChains
            .Where(IsValidChain)
            .Select(chain => chain.chainId)
            .ToList();
    }

    private void TryAutoBindPrimaryButtons()
    {
        if (root == null)
            return;

        var buttons = root.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                string methodName = button.onClick.GetPersistentMethodName(i);

                if (methodName == nameof(OnRandomSeedClicked))
                    randomSeedButton = button;
                else if (methodName == nameof(OnGenerateWorldClicked))
                    generateWorldButton = button;
            }
        }
    }

    private void RepositionPrimaryControls()
    {
        HideSeedControls();

        if (generateWorldButton != null)
        {
            RectTransform generateRect = generateWorldButton.transform as RectTransform;
            generateRect.anchorMin = new Vector2(0.5f, 0f);
            generateRect.anchorMax = new Vector2(0.5f, 0f);
            generateRect.pivot = new Vector2(0.5f, 0.5f);
            generateRect.anchoredPosition = new Vector2(0f, 58f);
            generateRect.sizeDelta = new Vector2(220f, 42f);
        }
    }

    private void HideSeedControls()
    {
        if (seedInput != null)
            seedInput.gameObject.SetActive(false);

        if (randomSeedButton != null)
            randomSeedButton.gameObject.SetActive(false);
    }

    private RegionLayout GetRegionLayout(WorldConfig config, int index)
    {
        string name = config != null ? config.name : $"World {index + 1}";

        switch (name)
        {
            case "Dust Frontier":
                return new RegionLayout
                {
                    displayName = "Dust Frontier",
                    description = "Red sand flats with dust fronts, mineral routes, and a harsh frontier atmosphere.",
                    position = new Vector2(-230f, 120f),
                    size = new Vector2(220f, 255f),
                    rotation = 0f,
                    shape = RegionShape.Cave,
                    idleColor = new Color(0.58f, 0.6f, 0.76f, 0.26f),
                    selectedColor = new Color(0.75f, 0.8f, 0.95f, 0.4f)
                };

            case "Crater Fields":
                return new RegionLayout
                {
                    displayName = "Crater Fields",
                    description = "Deep craters, broken terrain, and poor visibility. Built for hard expeditions and rare vein hunting.",
                    position = new Vector2(-30f, 140f),
                    size = new Vector2(160f, 260f),
                    rotation = 0f,
                    shape = RegionShape.Desert,
                    idleColor = new Color(0.2f, 0.18f, 0.72f, 0.24f),
                    selectedColor = new Color(0.42f, 0.4f, 1f, 0.38f)
                };

            case "Toxic Mire":
                return new RegionLayout
                {
                    displayName = "Toxic Mire",
                    description = "Flooded toxic lowlands with a green fog profile and a survival loop centered on organics.",
                    position = new Vector2(-255f, -130f),
                    size = new Vector2(230f, 230f),
                    rotation = 0f,
                    shape = RegionShape.Desert,
                    idleColor = new Color(0.74f, 0.68f, 0.27f, 0.3f),
                    selectedColor = new Color(0.96f, 0.88f, 0.39f, 0.42f)
                };

            case "Crystal Hollows":
                return new RegionLayout
                {
                    displayName = "Crystal Hollows",
                    description = "Crystal fractures and a cold underground tone. Best for greedier resource routes.",
                    position = new Vector2(-10f, -155f),
                    size = new Vector2(330f, 150f),
                    rotation = 0f,
                    shape = RegionShape.Mountains,
                    idleColor = new Color(0.72f, 0.46f, 0.18f, 0.28f),
                    selectedColor = new Color(0.95f, 0.63f, 0.27f, 0.42f)
                };

            case "Ruined Expanse":
                return new RegionLayout
                {
                    displayName = "Ruined Expanse",
                    description = "Ruins, plateaus, and ancient traces of civilization. Strong for combat runs and rare material recovery.",
                    position = new Vector2(235f, 0f),
                    size = new Vector2(325f, 255f),
                    rotation = 0f,
                    shape = RegionShape.Cave,
                    idleColor = new Color(0.82f, 0.84f, 0.86f, 0.2f),
                    selectedColor = new Color(0.98f, 0.99f, 1f, 0.34f)
                };

            default:
                return new RegionLayout
                {
                    displayName = name,
                    description = "New runtime world preset.",
                    position = GetFallbackPosition(index),
                    size = new Vector2(190f, 200f),
                    rotation = 0f,
                    shape = (RegionShape)(index % 3),
                    idleColor = new Color(0.25f, 0.55f, 0.58f, 0.24f),
                    selectedColor = new Color(0.45f, 0.8f, 0.84f, 0.38f)
                };
        }
    }

    private Vector2 GetFallbackPosition(int index)
    {
        Vector2[] positions =
        {
            new Vector2(-220f, 120f),
            new Vector2(-20f, 140f),
            new Vector2(-250f, -120f),
            new Vector2(-10f, -150f),
            new Vector2(240f, 0f),
            new Vector2(210f, 150f)
        };

        return positions[index % positions.Length];
    }

    private Sprite GetShapeSprite(RegionShape shape)
    {
        switch (shape)
        {
            case RegionShape.Cave:
                return caveRegionSprite;
            case RegionShape.Mountains:
                return mountainsRegionSprite;
            default:
                return desertRegionSprite;
        }
    }

    private string FormatRegionLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            return "Unknown";

        string[] parts = label.Split(' ');
        return parts.Length > 1
            ? string.Join("\n", parts)
            : label;
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

    private RectTransform CreateStretchRect(string name, RectTransform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.transform as RectTransform;
    }

    private TextMeshProUGUI CreateText(string name, Transform parent, float fontSize, FontStyles style, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var text = go.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = new Color(0.97f, 0.99f, 1f, 0.98f);
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;

        if (seedInput != null && seedInput.textComponent != null)
        {
            text.font = seedInput.textComponent.font;
            text.fontMaterial = seedInput.textComponent.fontMaterial;
        }

        return text;
    }

    private void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
