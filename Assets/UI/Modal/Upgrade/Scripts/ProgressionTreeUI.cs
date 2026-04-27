using System.Collections.Generic;
using System.Text;
using Features.Classes.Data;
using Features.Class.Net;
using Features.Input;
using Features.Passives.Domain;
using Features.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ProgressionTreeUI : PlayerBoundUIView, IUIScreen
{
    private static readonly Color RootBackgroundColor = new Color(0.11f, 0.13f, 0.13f, 0.98f);
    private static readonly Color AccentColor = new Color(0.43f, 0.86f, 0.94f, 0.95f);
    private static readonly Color CardBackgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.96f);
    private static readonly Color CardTitleColor = new Color(0.96f, 0.98f, 0.99f, 1f);
    private static readonly Color CardBodyColor = new Color(0.87f, 0.91f, 0.94f, 0.92f);

    [Header("Root")]
    [SerializeField] private GameObject root;
    [SerializeField] private Image rootImage;
    [SerializeField] private Outline rootOutline;

    [Header("Content")]
    [SerializeField] private Transform container;
    [SerializeField] private ProgressionNodeView nodePrefab;

    [Header("Title")]
    [SerializeField] private TMP_Text titleText;

    [Header("Reset Button")]
    [SerializeField] private Button resetButton;
    [SerializeField] private Image resetButtonImage;
    [SerializeField] private TMP_Text resetButtonLabel;

    [Header("Info Panel")]
    [SerializeField] private RectTransform infoPanelRect;
    [SerializeField] private Image infoPanelImage;
    [SerializeField] private TMP_Text infoTitleText;
    [SerializeField] private TMP_Text infoDescriptionText;

    private PlayerClassConfigSO currentClass;
    private RectTransform rootRect;
    private RectTransform containerRect;

    private string selectedNodeId;
    private ProgressionNodeSO hoveredNode;
    private RectTransform hoveredNodeRect;

    public InputMode Mode => InputMode.Dialog;

    protected override void OnEnable()
    {
        base.OnEnable();
        UIRegistry.I?.Register(this);
        CacheReferences();
        ApplyStaticStyle();
        BindButtons();
        HideInfoPanelImmediate();

        if (root != null)
            root.SetActive(false);
    }

    protected override void OnDisable()
    {
        resetButton.onClick.RemoveListener(OnResetAllClicked);
        UIRegistry.I?.Unregister(this);
        base.OnDisable();
    }

    private void BindButtons()
    {
        if (resetButton == null)
            return;

        resetButton.onClick.RemoveListener(OnResetAllClicked);
        resetButton.onClick.AddListener(OnResetAllClicked);
    }

    protected override void OnPlayerBound(GameObject player)
    {
        if (root != null)
            root.SetActive(false);
    }

    public void Show()
    {
        CacheReferences();
        ApplyStaticStyle();
        BindButtons();

        if (root != null)
            root.SetActive(true);

        ApplyRuntimeLayout();
        RefreshSelectionState();
        RefreshResetButtonState();
        HideInfoPanelImmediate();

        InputModeManager.I.SetMode(Mode);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);

        HideInfoPanelImmediate();
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

    public void Build(PlayerClassConfigSO cfg)
    {
        if (cfg == null || cfg.progression == null)
        {
            Debug.LogError("[ProgressionTreeUI] Missing progression config");
            return;
        }

        if (root == null || !root.scene.IsValid())
        {
            Debug.LogError("[ProgressionTreeUI] Root must reference a scene object.");
            return;
        }

        if (container == null || !container.gameObject.scene.IsValid())
        {
            Debug.LogError("[ProgressionTreeUI] Container must reference a scene object.");
            return;
        }

        if (nodePrefab == null)
        {
            Debug.LogError("[ProgressionTreeUI] Node prefab is not assigned.");
            return;
        }

        currentClass = cfg;

        CacheReferences();
        ApplyStaticStyle();
        UpdateTitle();
        ClearSpawnedNodes();
        HideInfoPanelImmediate();

        var state = PlayerProgressService.Instance.GetActiveCharacter();
        var nodes = cfg.progression.nodes;

        if (nodes == null || nodes.Count == 0)
        {
            selectedNodeId = null;
            RefreshResetButtonState();
            return;
        }

        NormalizeSelectedNode(nodes);

        foreach (var node in nodes)
        {
            if (node == null)
                continue;

            var view = Instantiate(nodePrefab, container);
            view.transform.localPosition = node.position;

            bool unlocked = node.passive != null && state.passives.Contains(node.passive.id);
            bool available = state.level >= node.requiredLevel;

            view.Init(
                node,
                unlocked,
                available,
                node.id == selectedNodeId,
                SelectNode,
                TryUnlock,
                TryRemove,
                ShowInfoPanel,
                HideInfoPanel);
        }

        RefreshSelectionState();
        RefreshResetButtonState();
    }

    private void TryUnlock(ProgressionNodeSO node)
    {
        if (BoundPlayer == null || node == null || node.passive == null)
            return;

        var state = PlayerProgressService.Instance.GetActiveCharacter();

        if (state.level < node.requiredLevel)
            return;

        if (state.passives.Contains(node.passive.id))
            return;

        selectedNodeId = node.id;
        state.passives.Add(node.passive.id);
        PlayerProgressService.Instance.Save();

        var net = BoundPlayer.GetComponent<PlayerStateNetAdapter>();
        if (net != null)
            net.ApplyClientProgressionServerRpc(state.passives.ToArray());

        Build(currentClass);
    }

    private void TryRemove(ProgressionNodeSO node)
    {
        if (BoundPlayer == null || node == null || node.passive == null)
            return;

        var state = PlayerProgressService.Instance.GetActiveCharacter();

        if (!state.passives.Contains(node.passive.id))
            return;

        selectedNodeId = node.id;
        state.passives.Remove(node.passive.id);
        PlayerProgressService.Instance.Save();

        var net = BoundPlayer.GetComponent<PlayerStateNetAdapter>();
        if (net != null)
            net.ApplyClientProgressionServerRpc(state.passives.ToArray());

        Build(currentClass);
    }

    public void OnResetAllClicked()
    {
        if (BoundPlayer == null)
            return;

        var state = PlayerProgressService.Instance.GetActiveCharacter();
        state.passives.Clear();
        PlayerProgressService.Instance.Save();

        var net = BoundPlayer.GetComponent<PlayerStateNetAdapter>();
        if (net != null)
            net.ApplyClientProgressionServerRpc(state.passives.ToArray());

        Build(currentClass);
    }

    private void SelectNode(ProgressionNodeSO node)
    {
        if (node == null)
            return;

        selectedNodeId = node.id;
        RefreshSelectionState();
    }

    private void ShowInfoPanel(ProgressionNodeSO node, RectTransform nodeRect)
    {
        if (node == null || nodeRect == null || infoPanelRect == null)
            return;

        hoveredNode = node;
        hoveredNodeRect = nodeRect;

        infoPanelRect.gameObject.SetActive(true);
        RefreshHoveredInfoPanel();
        UpdateInfoPanelPosition();
    }

    private void HideInfoPanel(ProgressionNodeSO node)
    {
        if (hoveredNode != node)
            return;

        HideInfoPanelImmediate();
    }

    private void HideInfoPanelImmediate()
    {
        hoveredNode = null;
        hoveredNodeRect = null;

        if (infoPanelRect != null)
            infoPanelRect.gameObject.SetActive(false);
    }

    private void RefreshHoveredInfoPanel()
    {
        if (infoTitleText == null || infoDescriptionText == null)
            return;

        if (hoveredNode == null)
        {
            HideInfoPanelImmediate();
            return;
        }

        var state = PlayerProgressService.Instance.GetActiveCharacter();
        bool unlocked = state != null &&
                        hoveredNode.passive != null &&
                        state.passives.Contains(hoveredNode.passive.id);
        bool available = state != null && state.level >= hoveredNode.requiredLevel;

        infoTitleText.text = GetNodeTitle(hoveredNode);
        infoDescriptionText.text = BuildNodeDescription(hoveredNode, unlocked, available);
    }

    private void UpdateInfoPanelPosition()
    {
        if (infoPanelRect == null || hoveredNodeRect == null || rootRect == null)
            return;

        Vector3 worldCenter = hoveredNodeRect.TransformPoint(hoveredNodeRect.rect.center);
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldCenter);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rootRect, screenPoint, null, out var localPoint);

        float panelWidth = infoPanelRect.rect.width > 1f ? infoPanelRect.rect.width : 320f;
        float halfRootWidth = rootRect.rect.width * 0.5f;

        float offsetX = hoveredNodeRect.rect.width * 0.5f + 18f;
        Vector2 targetPosition = localPoint + new Vector2(offsetX, 0f);

        infoPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
        infoPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
        infoPanelRect.pivot = new Vector2(0f, 0.5f);

        if (targetPosition.x + panelWidth > halfRootWidth - 16f)
        {
            infoPanelRect.pivot = new Vector2(1f, 0.5f);
            targetPosition = localPoint - new Vector2(offsetX, 0f);
        }

        infoPanelRect.anchoredPosition = targetPosition;
    }

    private void CacheReferences()
    {
        if (rootRect == null && root != null)
            rootRect = root.transform as RectTransform;

        if (containerRect == null && container != null)
            containerRect = container as RectTransform;

        if (rootImage == null && root != null)
            rootImage = root.GetComponent<Image>();

        if (rootOutline == null && root != null)
            rootOutline = root.GetComponent<Outline>();

        if (resetButtonImage == null && resetButton != null)
            resetButtonImage = resetButton.GetComponent<Image>();

        if (resetButtonLabel == null && resetButton != null)
            resetButtonLabel = resetButton.GetComponentInChildren<TMP_Text>(true);

        if (infoPanelImage == null && infoPanelRect != null)
            infoPanelImage = infoPanelRect.GetComponent<Image>();
    }

    private void ApplyStaticStyle()
    {
        if (rootImage != null)
        {
            rootImage.color = RootBackgroundColor;
            rootImage.raycastTarget = true;
        }

        if (rootOutline != null)
        {
            rootOutline.effectColor = AccentColor;
            rootOutline.effectDistance = new Vector2(1.5f, -1.5f);
            rootOutline.useGraphicAlpha = false;
        }

        if (resetButtonImage != null)
            resetButtonImage.color = new Color(0.11f, 0.12f, 0.13f, 0.9f);

        if (resetButtonLabel != null)
        {
            resetButtonLabel.text = "Reset";
            resetButtonLabel.fontSize = 19f;
            resetButtonLabel.alignment = TextAlignmentOptions.Center;
            resetButtonLabel.color = CardTitleColor;
        }

        if (titleText != null)
            titleText.color = CardTitleColor;

        if (infoPanelImage != null)
        {
            infoPanelImage.color = CardBackgroundColor;
            infoPanelImage.raycastTarget = false;
        }

        if (infoTitleText != null)
            infoTitleText.color = CardTitleColor;

        if (infoDescriptionText != null)
            infoDescriptionText.color = CardBodyColor;
    }

    private void ApplyRuntimeLayout()
    {
        if (rootRect == null || containerRect == null)
            return;

        float rootWidth = rootRect.rect.width > 1f ? rootRect.rect.width : 1280f;
        float rootHeight = rootRect.rect.height > 1f ? rootRect.rect.height : 720f;

        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = new Vector2(0f, -18f);
        containerRect.sizeDelta = new Vector2(
            Mathf.Max(rootWidth - 220f, 760f),
            Mathf.Max(rootHeight - 180f, 420f));

        if (titleText != null)
        {
            var titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -18f);
            titleRect.sizeDelta = new Vector2(Mathf.Max(rootWidth - 260f, 420f), 42f);
        }

        if (infoPanelRect != null)
        {
            infoPanelRect.sizeDelta = new Vector2(Mathf.Min(rootWidth * 0.34f, 420f), 150f);

            if (hoveredNodeRect != null && infoPanelRect.gameObject.activeSelf)
                UpdateInfoPanelPosition();
        }

        if (resetButton != null)
        {
            var resetRect = resetButton.transform as RectTransform;
            if (resetRect != null)
            {
                resetRect.anchorMin = new Vector2(0f, 1f);
                resetRect.anchorMax = new Vector2(0f, 1f);
                resetRect.pivot = new Vector2(0f, 1f);
                resetRect.anchoredPosition = new Vector2(28f, -24f);
                resetRect.sizeDelta = new Vector2(126f, 34f);
            }
        }
    }

    private void ClearSpawnedNodes()
    {
        if (container == null)
            return;

        var toRemove = new List<GameObject>();

        foreach (Transform child in container)
        {
            if (child.GetComponent<ProgressionNodeView>() != null)
                toRemove.Add(child.gameObject);
        }

        foreach (var go in toRemove)
            Destroy(go);
    }

    private void RefreshSelectionState()
    {
        if (container == null)
            return;

        foreach (var view in container.GetComponentsInChildren<ProgressionNodeView>(true))
            view.SetSelected(view.NodeId == selectedNodeId);
    }

    private void RefreshResetButtonState()
    {
        if (resetButton == null)
            return;

        var state = PlayerProgressService.Instance.GetActiveCharacter();
        bool hasUnlockedNodes = state != null && state.passives != null && state.passives.Count > 0;

        resetButton.interactable = hasUnlockedNodes;

        if (resetButtonImage != null)
        {
            resetButtonImage.color = hasUnlockedNodes
                ? new Color(0.11f, 0.12f, 0.13f, 0.9f)
                : new Color(0.11f, 0.12f, 0.13f, 0.45f);
        }
    }

    private void UpdateTitle()
    {
        if (titleText == null)
            return;

        string className = currentClass != null && !string.IsNullOrWhiteSpace(currentClass.displayName)
            ? currentClass.displayName
            : "Tech";

        titleText.text = $"Upgrades {className}";
    }

    private void NormalizeSelectedNode(List<ProgressionNodeSO> nodes)
    {
        if (!string.IsNullOrWhiteSpace(selectedNodeId))
        {
            foreach (var node in nodes)
            {
                if (node != null && node.id == selectedNodeId)
                    return;
            }
        }

        selectedNodeId = null;

        foreach (var node in nodes)
        {
            if (node == null)
                continue;

            selectedNodeId = node.id;
            return;
        }
    }

    private string GetNodeTitle(ProgressionNodeSO node)
    {
        if (node == null)
            return "Upgrade";

        if (!string.IsNullOrWhiteSpace(node.displayName))
            return node.displayName.Trim();

        if (node.passive != null && !string.IsNullOrWhiteSpace(node.passive.name))
            return HumanizeLabel(node.passive.name);

        if (!string.IsNullOrWhiteSpace(node.id))
            return HumanizeLabel(node.id);

        return "Upgrade";
    }

    private string BuildNodeDescription(ProgressionNodeSO node, bool unlocked, bool available)
    {
        var builder = new StringBuilder();

        string description = GetNodeBody(node);
        if (!string.IsNullOrWhiteSpace(description))
            builder.Append(description.Trim());

        if (builder.Length > 0)
            builder.Append("\n\n");

        builder.Append($"<color=#AFC4CC>Required level: {node.requiredLevel}</color>\n");

        string status = unlocked
            ? "Status: unlocked."
            : available
                ? "Status: available now."
                : $"Status: locked.";

        builder.Append($"<color=#74D7EB>{status}</color>");

        if (unlocked)
            builder.Append("\n<color=#A1B2BA>Right click this node to remove it.</color>");

        return builder.ToString();
    }

    private string GetNodeBody(ProgressionNodeSO node)
    {
        if (node == null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(node.description))
            return node.description;

        if (node.passive == null)
            return "Unlocks a new class upgrade.";

        var parts = new List<string>();

        if (node.passive.effects != null)
        {
            foreach (var effect in node.passive.effects)
            {
                if (effect == null)
                    continue;

                parts.Add(DescribePassiveEffect(effect));
            }
        }

        if (node.passive.abilityModifiers != null)
        {
            foreach (var modifier in node.passive.abilityModifiers)
            {
                if (modifier == null)
                    continue;

                parts.Add(HumanizeLabel(modifier.name) + ".");
            }
        }

        if (parts.Count == 0)
            parts.Add("Unlocks a new class upgrade.");

        return string.Join("\n", parts);
    }

    private string DescribePassiveEffect(PassiveEffectSO effect)
    {
        if (effect is PassiveEffect_ApplyBuffSO applyBuff && applyBuff.buff != null)
        {
            string buffName = !string.IsNullOrWhiteSpace(applyBuff.buff.displayName)
                ? applyBuff.buff.displayName
                : HumanizeLabel(applyBuff.buff.name);

            return $"{buffName}.";
        }

        return HumanizeLabel(effect.name) + ".";
    }

    private static string HumanizeLabel(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var builder = new StringBuilder(value.Length + 8);
        char previous = '\0';

        foreach (char rawChar in value)
        {
            char current = rawChar == '_' || rawChar == '-' ? ' ' : rawChar;

            if (builder.Length > 0 &&
                current != ' ' &&
                previous != ' ' &&
                ((char.IsUpper(current) && (char.IsLower(previous) || char.IsDigit(previous))) ||
                 (char.IsDigit(current) && char.IsLetter(previous))))
            {
                builder.Append(' ');
            }

            builder.Append(current);
            previous = current;
        }

        return builder.ToString().Trim();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (root != null && root.activeInHierarchy)
        {
            ApplyRuntimeLayout();

            if (hoveredNodeRect != null && infoPanelRect != null && infoPanelRect.gameObject.activeSelf)
                UpdateInfoPanelPosition();
        }
    }
}
