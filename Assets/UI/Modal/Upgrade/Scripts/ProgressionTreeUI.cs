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
    private struct DecorativeNodeSpec
    {
        public readonly Vector2 position;
        public readonly float size;
        public readonly bool filled;
        public readonly float alpha;

        public DecorativeNodeSpec(float x, float y, float size, bool filled, float alpha)
        {
            position = new Vector2(x, y);
            this.size = size;
            this.filled = filled;
            this.alpha = alpha;
        }
    }

    private struct DecorativeLineSpec
    {
        public readonly int startNodeIndex;
        public readonly int endNodeIndex;
        public readonly float alpha;

        public DecorativeLineSpec(int startNodeIndex, int endNodeIndex, float alpha)
        {
            this.startNodeIndex = startNodeIndex;
            this.endNodeIndex = endNodeIndex;
            this.alpha = alpha;
        }
    }

    private static readonly Color RootBackgroundColor = new Color(0.11f, 0.13f, 0.13f, 0.98f);
    private static readonly Color AccentColor = new Color(0.43f, 0.86f, 0.94f, 0.95f);
    private static readonly Color CardBackgroundColor = new Color(0.12f, 0.12f, 0.12f, 0.96f);
    private static readonly Color CardTitleColor = new Color(0.96f, 0.98f, 0.99f, 1f);
    private static readonly Color CardBodyColor = new Color(0.87f, 0.91f, 0.94f, 0.92f);
    private static readonly DecorativeNodeSpec[] BackdropNodes =
    {
        new DecorativeNodeSpec(-0.32f, 0.16f, 78f, false, 0.86f),
        new DecorativeNodeSpec(-0.24f, 0.02f, 36f, false, 0.84f),
        new DecorativeNodeSpec(-0.13f, 0.02f, 36f, false, 0.84f),
        new DecorativeNodeSpec(0f, 0.02f, 72f, false, 0.9f),
        new DecorativeNodeSpec(0.13f, 0.02f, 36f, false, 0.84f),
        new DecorativeNodeSpec(0.24f, 0.02f, 36f, false, 0.84f),
        new DecorativeNodeSpec(0.32f, 0.16f, 78f, false, 0.86f),
        new DecorativeNodeSpec(-0.13f, -0.16f, 72f, false, 0.88f),
        new DecorativeNodeSpec(0.18f, -0.16f, 72f, false, 0.88f),
        new DecorativeNodeSpec(-0.05f, -0.30f, 36f, false, 0.84f),
        new DecorativeNodeSpec(0.05f, -0.30f, 36f, false, 0.84f),
        new DecorativeNodeSpec(0f, -0.40f, 42f, true, 0.96f)
    };
    private static readonly DecorativeLineSpec[] BackdropLines =
    {
        new DecorativeLineSpec(9, 11, 0.64f),
        new DecorativeLineSpec(10, 11, 0.64f),
        new DecorativeLineSpec(7, 9, 0.36f),
        new DecorativeLineSpec(8, 10, 0.36f)
    };

    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private Transform container;
    [SerializeField] private ProgressionNodeView nodePrefab;

    private PlayerClassConfigSO currentClass;
    private RectTransform rootRect;
    private RectTransform containerRect;
    private Image rootImage;
    private Outline rootOutline;
    private Button resetButton;
    private Image resetButtonImage;
    private TMP_Text resetButtonLabel;
    private RectTransform backdropRect;
    private readonly List<RectTransform> backdropNodeRects = new List<RectTransform>();
    private readonly List<Image> backdropNodeImages = new List<Image>();
    private readonly List<RectTransform> backdropLineRects = new List<RectTransform>();
    private TMP_Text titleText;
    private RectTransform infoPanelRect;
    private Image infoPanelImage;
    private TMP_Text infoTitleText;
    private TMP_Text infoDescriptionText;
    private string selectedNodeId;

    public InputMode Mode => InputMode.Dialog;

    protected override void OnEnable()
    {
        base.OnEnable();
        UIRegistry.I?.Register(this);
        EnsureRuntimeUi();
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
        EnsureRuntimeUi();
        root.SetActive(true);
        ApplyRuntimeLayout();
        RefreshSelectionState();
        RefreshInfoPanel();
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

        currentClass = cfg;
        EnsureRuntimeUi();
        UpdateTitle();

        foreach (Transform child in container)
            Destroy(child.gameObject);

        var state = PlayerProgressService.Instance.GetActiveCharacter();
        var nodes = cfg.progression.nodes;

        if (nodes == null || nodes.Count == 0)
        {
            selectedNodeId = null;
            RefreshInfoPanel();
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
                TryRemove);
        }

        RefreshSelectionState();
        RefreshResetButtonState();
        RefreshInfoPanel();
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
        net.RefreshPassives();

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
        net.RefreshPassives();

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
        net.RefreshPassives();

        Build(currentClass);
    }

    private void SelectNode(ProgressionNodeSO node)
    {
        if (node == null)
            return;

        selectedNodeId = node.id;
        RefreshSelectionState();
        RefreshInfoPanel();
    }

    private void EnsureRuntimeUi()
    {
        if (root == null)
            return;

        if (rootRect == null)
            rootRect = root.transform as RectTransform;

        if (containerRect == null)
            containerRect = container as RectTransform;

        if (rootImage == null)
            rootImage = root.GetComponent<Image>();

        if (rootImage != null)
        {
            rootImage.color = RootBackgroundColor;
            rootImage.raycastTarget = true;
        }

        if (rootOutline == null)
            rootOutline = root.GetComponent<Outline>();
        if (rootOutline == null)
            rootOutline = root.AddComponent<Outline>();

        rootOutline.effectColor = AccentColor;
        rootOutline.effectDistance = new Vector2(1.5f, -1.5f);
        rootOutline.useGraphicAlpha = false;

        EnsureBackdrop();
        EnsureResetButton();
        EnsureTitleText();
        EnsureInfoPanel();
    }

    private void EnsureResetButton()
    {
        if (resetButton != null)
            return;

        var resetTransform = root.transform.Find("Reset All");
        if (resetTransform == null)
            return;

        resetButton = resetTransform.GetComponent<Button>();
        if (resetButton == null)
            return;

        resetButtonImage = resetButton.GetComponent<Image>();
        resetButtonLabel = resetButton.GetComponentInChildren<TMP_Text>(true);

        if (resetButtonImage != null)
        {
            resetButtonImage.color = new Color(0.11f, 0.12f, 0.13f, 0.9f);

            var buttonOutline = resetButton.GetComponent<Outline>();
            if (buttonOutline == null)
                buttonOutline = resetButton.gameObject.AddComponent<Outline>();

            buttonOutline.effectColor = new Color(0.35f, 0.73f, 0.8f, 0.85f);
            buttonOutline.effectDistance = new Vector2(1f, -1f);
            buttonOutline.useGraphicAlpha = false;
        }

        if (resetButtonLabel != null)
        {
            resetButtonLabel.text = "Reset";
            resetButtonLabel.fontSize = 19f;
            resetButtonLabel.alignment = TextAlignmentOptions.Center;
            resetButtonLabel.color = CardTitleColor;
        }
    }

    private void EnsureTitleText()
    {
        if (titleText != null)
            return;

        titleText = CreateText("UpgradeTitle", root.transform, 34f, FontStyles.Bold, TextAlignmentOptions.Center);
        titleText.color = CardTitleColor;
    }

    private void EnsureBackdrop()
    {
        if (backdropRect != null || root == null || nodePrefab == null)
            return;

        var backdropGo = new GameObject("UpgradeBackdrop", typeof(RectTransform));
        backdropGo.transform.SetParent(root.transform, false);

        backdropRect = backdropGo.GetComponent<RectTransform>();
        backdropRect.SetSiblingIndex(0);

        for (int i = 0; i < BackdropLines.Length; i++)
        {
            var lineGo = new GameObject("BackdropLine", typeof(RectTransform), typeof(Image));
            lineGo.transform.SetParent(backdropRect, false);

            var lineRect = lineGo.GetComponent<RectTransform>();
            var lineImage = lineGo.GetComponent<Image>();
            lineImage.color = new Color(AccentColor.r, AccentColor.g, AccentColor.b, BackdropLines[i].alpha);
            lineImage.raycastTarget = false;

            backdropLineRects.Add(lineRect);
        }

        for (int i = 0; i < BackdropNodes.Length; i++)
        {
            var nodeGo = new GameObject("BackdropNode", typeof(RectTransform), typeof(Image));
            nodeGo.transform.SetParent(backdropRect, false);

            var nodeRect = nodeGo.GetComponent<RectTransform>();
            var nodeImage = nodeGo.GetComponent<Image>();
            nodeImage.sprite = GetBackdropSprite(BackdropNodes[i]);
            nodeImage.color = new Color(1f, 1f, 1f, BackdropNodes[i].alpha);
            nodeImage.preserveAspect = true;
            nodeImage.raycastTarget = false;

            backdropNodeRects.Add(nodeRect);
            backdropNodeImages.Add(nodeImage);
        }

        if (container != null)
            container.SetSiblingIndex(1);
    }

    private void EnsureInfoPanel()
    {
        if (infoPanelRect != null)
            return;

        var panelGo = new GameObject("UpgradeInfoPanel", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(root.transform, false);

        infoPanelRect = panelGo.GetComponent<RectTransform>();
        infoPanelImage = panelGo.GetComponent<Image>();
        infoPanelImage.color = CardBackgroundColor;
        infoPanelImage.raycastTarget = false;

        infoTitleText = CreateText("UpgradeInfoTitle", panelGo.transform, 22f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        infoTitleText.color = CardTitleColor;
        infoTitleText.rectTransform.offsetMin = new Vector2(18f, 78f);
        infoTitleText.rectTransform.offsetMax = new Vector2(-18f, -18f);

        infoDescriptionText = CreateText("UpgradeInfoBody", panelGo.transform, 16f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        infoDescriptionText.color = CardBodyColor;
        infoDescriptionText.rectTransform.offsetMin = new Vector2(18f, 18f);
        infoDescriptionText.rectTransform.offsetMax = new Vector2(-18f, -48f);
    }

    private void ApplyRuntimeLayout()
    {
        if (rootRect == null || containerRect == null)
            return;

        float rootWidth = rootRect.rect.width > 1f ? rootRect.rect.width : 1280f;
        float rootHeight = rootRect.rect.height > 1f ? rootRect.rect.height : 720f;

        ApplyBackdropLayout(rootWidth, rootHeight);

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
            infoPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            infoPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            infoPanelRect.pivot = new Vector2(0.5f, 0.5f);
            infoPanelRect.anchoredPosition = new Vector2(rootWidth * 0.07f, -Mathf.Min(rootHeight * 0.16f, 135f));
            infoPanelRect.sizeDelta = new Vector2(Mathf.Min(rootWidth * 0.34f, 420f), 150f);
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

    private void ApplyBackdropLayout(float rootWidth, float rootHeight)
    {
        if (backdropRect == null)
            return;

        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = Vector2.zero;
        backdropRect.offsetMax = Vector2.zero;

        for (int i = 0; i < backdropNodeRects.Count && i < BackdropNodes.Length; i++)
        {
            var spec = BackdropNodes[i];
            var rect = backdropNodeRects[i];
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = GetBackdropPosition(spec, rootWidth, rootHeight);
            rect.sizeDelta = new Vector2(spec.size, spec.size);

            if (i < backdropNodeImages.Count)
                backdropNodeImages[i].sprite = GetBackdropSprite(spec);
        }

        for (int i = 0; i < backdropLineRects.Count && i < BackdropLines.Length; i++)
        {
            var lineSpec = BackdropLines[i];
            var lineRect = backdropLineRects[i];
            var start = GetBackdropPosition(BackdropNodes[lineSpec.startNodeIndex], rootWidth, rootHeight);
            var end = GetBackdropPosition(BackdropNodes[lineSpec.endNodeIndex], rootWidth, rootHeight);
            LayoutLine(lineRect, start, end);
        }
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
            resetButtonImage.color = hasUnlockedNodes
                ? new Color(0.11f, 0.12f, 0.13f, 0.9f)
                : new Color(0.11f, 0.12f, 0.13f, 0.45f);
    }

    private void RefreshInfoPanel()
    {
        if (infoTitleText == null || infoDescriptionText == null)
            return;

        var selectedNode = FindSelectedNode();
        if (selectedNode == null)
        {
            infoTitleText.text = "Upgrade title";
            infoDescriptionText.text = "Upgrade descriptions";
            return;
        }

        var state = PlayerProgressService.Instance.GetActiveCharacter();
        bool unlocked = state != null &&
            selectedNode.passive != null &&
            state.passives.Contains(selectedNode.passive.id);
        bool available = state != null && state.level >= selectedNode.requiredLevel;

        infoTitleText.text = GetNodeTitle(selectedNode);
        infoDescriptionText.text = BuildNodeDescription(selectedNode, unlocked, available);
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

    private ProgressionNodeSO FindSelectedNode()
    {
        if (currentClass?.progression?.nodes == null || string.IsNullOrWhiteSpace(selectedNodeId))
            return null;

        foreach (var node in currentClass.progression.nodes)
        {
            if (node != null && node.id == selectedNodeId)
                return node;
        }

        return null;
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

        string status = unlocked
            ? "Status: unlocked."
            : available
                ? "Status: available now."
                : $"Status: requires level {node.requiredLevel}.";
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

    private Sprite GetBackdropSprite(DecorativeNodeSpec spec)
    {
        if (nodePrefab == null)
            return null;

        if (spec.filled)
            return nodePrefab.UnlockedSprite;

        return spec.size >= 70f
            ? nodePrefab.LargeAvailableSprite
            : nodePrefab.AvailableSprite;
    }

    private static Vector2 GetBackdropPosition(DecorativeNodeSpec spec, float rootWidth, float rootHeight)
    {
        return new Vector2(rootWidth * spec.position.x, rootHeight * spec.position.y);
    }

    private static void LayoutLine(RectTransform lineRect, Vector2 start, Vector2 end)
    {
        if (lineRect == null)
            return;

        var delta = end - start;
        float length = delta.magnitude;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0f, 0.5f);
        lineRect.anchoredPosition = start;
        lineRect.sizeDelta = new Vector2(length, 2f);
        lineRect.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private TMP_Text CreateText(
        string objectName,
        Transform parent,
        float fontSize,
        FontStyles fontStyle,
        TextAlignmentOptions alignment)
    {
        var go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.text = string.Empty;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.Normal;

        var rect = text.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return text;
    }

    private void OnRectTransformDimensionsChange()
    {
        if (root != null && root.activeInHierarchy)
            ApplyRuntimeLayout();
    }
}
