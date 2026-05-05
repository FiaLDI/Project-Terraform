using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCardView : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI classLabel;
    public TextMeshProUGUI nicknameLabel;
    public TextMeshProUGUI levelLabel;
    public TextMeshProUGUI specLabel;
    public Image icon;
    public Image Active;

    private int _index;
    private System.Action<int> _onSelect;

    public void Setup(PlayerCharacterState state, int index, System.Action<int> onSelect, int selectedIndex)
    {
        _index = index;
        _onSelect = onSelect;

        classLabel.text = state.classId.ToUpper();
        levelLabel.text = "LVL " + state.level;

        specLabel.text = state.specializationId == null
            ? "No spec"
            : state.specializationId;
        
        nicknameLabel.text = state.nickname;
        SetSelected(selectedIndex == index);
    }

    public void SetupExpedition(
        ExpeditionSaveData state,
        string activePlanetLabel,
        string progressLabel,
        int index,
        System.Action<int> onSelect,
        int selectedIndex)
    {
        _index = index;
        _onSelect = onSelect;

        classLabel.text = state != null && !string.IsNullOrWhiteSpace(state.displayName)
            ? state.displayName
            : "Expedition";
        levelLabel.text = "SHIP LVL " + (state != null ? state.shipLevel : 1);
        specLabel.text = string.IsNullOrWhiteSpace(progressLabel)
            ? "No progress yet"
            : progressLabel;
        nicknameLabel.text = string.IsNullOrWhiteSpace(activePlanetLabel)
            ? "No active planet"
            : activePlanetLabel;

        SetSelected(selectedIndex == index);
    }

    public void SetSelected(bool isSelected)
    {
        if (Active != null)
            Active.gameObject.SetActive(isSelected);
    }

    public void OnClick()
    {
        _onSelect?.Invoke(_index);
    }
}
