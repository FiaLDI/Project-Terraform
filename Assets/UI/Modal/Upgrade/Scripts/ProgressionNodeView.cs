using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ProgressionNodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image background;

    [Header("Node Art")]
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private Sprite availableSprite;
    [SerializeField] private Sprite largeAvailableSprite;

    private static readonly Color AvailableTint = new Color(1f, 1f, 1f, 0.95f);
    private static readonly Color LockedTint = new Color(0.63f, 0.88f, 0.96f, 0.48f);
    private static readonly Color SelectedTint = new Color(1f, 1f, 1f, 1f);

    private ProgressionNodeSO node;
    private RectTransform rectTransform;
    private System.Action<ProgressionNodeSO> onSelected;
    private System.Action<ProgressionNodeSO> onUnlock;
    private System.Action<ProgressionNodeSO> onRemove;
    private System.Action<ProgressionNodeSO, RectTransform> onHoverEnter;
    private System.Action<ProgressionNodeSO> onHoverExit;
    private bool isUnlocked;
    private bool isAvailable;

    public string NodeId => node != null ? node.id : string.Empty;
    public Sprite UnlockedSprite => unlockedSprite;
    public Sprite AvailableSprite => availableSprite;
    public Sprite LargeAvailableSprite => largeAvailableSprite;

    public void Init(
        ProgressionNodeSO node,
        bool unlocked,
        bool available,
        bool selected,
        System.Action<ProgressionNodeSO> onSelected,
        System.Action<ProgressionNodeSO> onUnlock,
        System.Action<ProgressionNodeSO> onRemove,
        System.Action<ProgressionNodeSO, RectTransform> onHoverEnter,
        System.Action<ProgressionNodeSO> onHoverExit)
    {
        this.node = node;
        this.onSelected = onSelected;
        this.onUnlock = onUnlock;
        this.onRemove = onRemove;
        this.onHoverEnter = onHoverEnter;
        this.onHoverExit = onHoverExit;
        isUnlocked = unlocked;
        isAvailable = available;

        if (rectTransform == null)
            rectTransform = transform as RectTransform;

        RefreshVisuals(selected);
    }

    public void SetSelected(bool selected)
    {
        RefreshVisuals(selected);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (node == null || rectTransform == null)
            return;

        onHoverEnter?.Invoke(node, rectTransform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (node == null)
            return;

        onHoverExit?.Invoke(node);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (node == null)
            return;

        onSelected?.Invoke(node);

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (isUnlocked)
                onRemove?.Invoke(node);

            return;
        }

        if (!isUnlocked && isAvailable)
            onUnlock?.Invoke(node);
    }

    private void RefreshVisuals(bool selected)
    {
        if (node == null || background == null || rectTransform == null)
            return;

        float nodeSize = node.uiSize > 1f ? node.uiSize : 72f;
        bool useLargeSprite = nodeSize >= 90f;

        rectTransform.sizeDelta = new Vector2(nodeSize, nodeSize);
        rectTransform.localScale = selected ? Vector3.one * 1.08f : Vector3.one;

        background.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        background.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        background.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        background.rectTransform.anchoredPosition = Vector2.zero;
        background.rectTransform.sizeDelta = new Vector2(nodeSize, nodeSize);

        background.sprite = isUnlocked
            ? unlockedSprite
            : useLargeSprite
                ? largeAvailableSprite
                : availableSprite;

        background.preserveAspect = true;
        background.color = GetNodeColor(selected);
        background.raycastTarget = true;
    }

    private Color GetNodeColor(bool selected)
    {
        if (selected)
            return SelectedTint;

        if (isUnlocked || isAvailable)
            return AvailableTint;

        return LockedTint;
    }
}
