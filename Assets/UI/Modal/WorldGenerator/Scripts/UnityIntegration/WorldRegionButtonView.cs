using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class WorldRegionButtonView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform rootRect;
    [SerializeField] private PolygonGlowButton polygonButton;
    [SerializeField] private Image baseImage;
    [SerializeField] private Image glowImage;
    [SerializeField] private TextMeshProUGUI titleLabel;

    [Header("Style")]
    [SerializeField] private WorldRegionButtonStyle style;

    public PolygonGlowButton Button => polygonButton;

    public void Bind(WorldSelectionEntry entry, Action onClick)
    {
        if (entry == null)
            return;

        if (rootRect == null)
            rootRect = transform as RectTransform;

        ApplyLayout(entry);
        ApplyVisuals(entry);
        ApplyButtonState(entry, onClick);
        ApplyLabel(entry);
        ApplyInitialVisualState(entry);
    }

    private void ApplyLayout(WorldSelectionEntry entry)
    {
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = entry.position;
        rootRect.sizeDelta = entry.size;
        rootRect.localEulerAngles = new Vector3(0f, 0f, entry.rotation);
    }

    private void ApplyVisuals(WorldSelectionEntry entry)
    {
        if (baseImage != null)
        {
            if (entry.regionSprite != null)
            {
                baseImage.sprite = entry.regionSprite;
                baseImage.useSpriteMesh = true;
            }

            baseImage.rectTransform.sizeDelta = entry.size;
            baseImage.rectTransform.anchoredPosition = Vector2.zero;
            baseImage.color = entry.idleColor;
        }

        if (glowImage != null)
        {
            if (entry.regionSprite != null)
            {
                glowImage.sprite = entry.regionSprite;
                glowImage.useSpriteMesh = true;
            }

            glowImage.rectTransform.sizeDelta = entry.size;
            glowImage.rectTransform.anchoredPosition = Vector2.zero;
            glowImage.color = entry.idleColor;
        }
    }

    private void ApplyButtonState(WorldSelectionEntry entry, Action onClick)
    {
        if (polygonButton == null)
            return;

        polygonButton.baseImage = baseImage;
        polygonButton.glowImage = glowImage;
        polygonButton.idleColor = entry.idleColor;
        polygonButton.selectedColor = entry.selectedColor;
        polygonButton.lockedColor = entry.lockedColor.a > 0f
            ? entry.lockedColor
            : GetDefaultLockedColor();

        polygonButton.hoverHighlight = style != null ? style.hoverHighlight : 1.15f;
        polygonButton.selectedHighlight = style != null ? style.selectedHighlight : 0.6f;
        polygonButton.fadeSpeed = style != null ? style.fadeSpeed : 8f;

        polygonButton.onClick.RemoveAllListeners();
        if (onClick != null)
            polygonButton.onClick.AddListener(() => onClick());
    }

    private void ApplyLabel(WorldSelectionEntry entry)
    {
        if (titleLabel == null)
            return;

        titleLabel.text = FormatRegionLabel(GetDisplayName(entry));
        titleLabel.raycastTarget = false;
        titleLabel.color = style != null
            ? style.labelColor
            : new Color(0.96f, 0.98f, 0.99f, 0.96f);
    }

    private void ApplyInitialVisualState(WorldSelectionEntry entry)
    {
        if (polygonButton != null)
        {
            polygonButton.SetState(ButtonState.Idle);
        }
        else
        {
            if (baseImage != null)
                baseImage.color = entry.idleColor;

            if (glowImage != null)
                glowImage.color = entry.idleColor;
        }
    }

    private Color GetDefaultLockedColor()
    {
        return style != null
            ? style.defaultLockedColor
            : new Color(0.08f, 0.18f, 0.18f, 0.18f);
    }

    private string GetDisplayName(WorldSelectionEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.displayName))
            return entry.displayName;

        if (entry.worldConfig != null && !string.IsNullOrWhiteSpace(entry.worldConfig.name))
            return entry.worldConfig.name;

        return "Unknown";
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
}
