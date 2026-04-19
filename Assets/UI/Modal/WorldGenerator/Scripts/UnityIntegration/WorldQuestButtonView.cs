using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.Quests.Data;

public sealed class WorldQuestButtonView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI titleLabel;

    [Header("Style")]
    [SerializeField] private WorldQuestButtonStyle style;

    public Button Button => button;

    public void Bind(QuestAsset quest, Action onClick)
    {
        if (button == null)
            button = GetComponent<Button>();

        if (backgroundImage == null && button != null)
            backgroundImage = button.targetGraphic as Image;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            if (onClick != null)
                button.onClick.AddListener(() => onClick());
        }

        if (titleLabel != null)
        {
            titleLabel.text = GetQuestDisplayName(quest);
            titleLabel.raycastTarget = false;
            titleLabel.color = style != null
                ? style.labelColor
                : new Color(0.97f, 0.99f, 1f, 0.98f);
        }

        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        if (backgroundImage == null)
            return;

        if (style != null)
        {
            backgroundImage.color = isSelected
                ? style.selectedBackgroundColor
                : style.normalBackgroundColor;
        }
        else
        {
            backgroundImage.color = isSelected
                ? new Color(0.27f, 0.43f, 0.48f, 0.98f)
                : new Color(0.15f, 0.19f, 0.22f, 0.96f);
        }
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
}
