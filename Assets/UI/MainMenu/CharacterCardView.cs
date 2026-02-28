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

    private int _index;
    private System.Action<int> _onSelect;

    public void Setup(PlayerCharacterState state, int index, System.Action<int> onSelect)
    {
        _index = index;
        _onSelect = onSelect;

        classLabel.text = state.classId.ToUpper();
        levelLabel.text = "LVL " + state.level;

        specLabel.text = state.specializationId == null
            ? "No spec"
            : state.specializationId;
        
        nicknameLabel.text = state.nickname;

        // Заглушка: можно будет ставить icons[state.classId]
        // icon.sprite = ...
    }

    public void OnClick()
    {
        _onSelect?.Invoke(_index);
    }
}
