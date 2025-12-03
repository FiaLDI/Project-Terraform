using UnityEngine;
using UnityEngine.UI;
using Features.Classes.Data;
using TMPro;

public class ClassSelectionButton : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI desc;
    public Button button;

    private PlayerClassConfigSO _config;

    public System.Action onClick;

    public void Set(PlayerClassConfigSO cfg)
    {
        _config = cfg;
        title.text = cfg.displayName;
        desc.text = cfg.description;

        button.onClick.AddListener(() => onClick?.Invoke());
    }
}
