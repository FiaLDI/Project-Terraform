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

    public void Setup(PlayerCharacterState state, int index, System.Action<int> onSelect, float _selectedIndex)
    {
        _index = index;
        _onSelect = onSelect;

        classLabel.text = state.classId.ToUpper();
        levelLabel.text = "LVL " + state.level;

        specLabel.text = state.specializationId == null
            ? "No spec"
            : state.specializationId;
        
        nicknameLabel.text = state.nickname;
        var isActive = _selectedIndex == index;

        if (isActive)
            Active.gameObject.SetActive(true);
        else {
            Active.gameObject.SetActive(false);
        }
    }

    public void OnClick()
    {
        _onSelect?.Invoke(_index);
    }
}
