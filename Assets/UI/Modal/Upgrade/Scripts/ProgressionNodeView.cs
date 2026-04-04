using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressionNodeView : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image lockIcon;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Button button;
    [SerializeField] private Button removeButton;

    public void Init(
        ProgressionNodeSO node,
        bool unlocked,
        bool available,
        System.Action onClick,
        System.Action onRemove)
    {
        levelText.text = $"Lv {node.requiredLevel}";

        lockIcon.gameObject.SetActive(!unlocked);

        if (unlocked)
            background.color = Color.green;
        else if (available)
            background.color = Color.yellow;
        else
            background.color = Color.gray;

        button.interactable = available && !unlocked;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick());

        removeButton.gameObject.SetActive(unlocked);
        removeButton.onClick.RemoveAllListeners();
        removeButton.onClick.AddListener(() => onRemove());
    }
}
