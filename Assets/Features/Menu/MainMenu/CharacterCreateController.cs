using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CharacterCreateController : MonoBehaviour
{
    public TMP_Dropdown classDropdown;
    public TMP_InputField nicknameInput;

    private PlayerProgressService _progress;

    private void Start()
    {
        _progress = PlayerProgressService.Instance;

        classDropdown.ClearOptions();
        classDropdown.AddOptions(new List<string> { "engineer", "miner", "fighter", "comms" });
    }

    public void OnCreate()
    {
        string nick = nicknameInput.text.Trim();
        string cls = classDropdown.options[classDropdown.value].text;

        if (nick.Length == 0)
        {
            Debug.LogWarning("Nickname cannot be empty");
            return;
        }

        _progress.AddCharacter(cls, nick);

        MainMenuFSM.Instance.Switch(MainMenuStateId.CharacterSelect);
    }

    public void OnCancel()
        => MainMenuFSM.Instance.Switch(MainMenuStateId.CharacterSelect);
}
