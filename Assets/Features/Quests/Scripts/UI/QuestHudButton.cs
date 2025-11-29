using UnityEngine;
using UnityEngine.UI;
using Quests;

public class QuestHudButton : MonoBehaviour
{
    public QuestUI questUI;
    public Image icon;
    public Sprite iconOpen;
    public Sprite iconClosed;

    public void Toggle()
    {
        questUI.ToggleHudQuestPanel();
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (questUI.hudIsOpen)
            icon.sprite = iconClosed;
        else
            icon.sprite = iconOpen;
    }
}
