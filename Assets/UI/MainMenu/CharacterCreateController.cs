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
        _progress = EnsureProgressService();
        EnterCharacterCreate();
    }

    public void EnterCharacterCreate()
    {
        if (classDropdown != null)
        {
            classDropdown.gameObject.SetActive(true);
            classDropdown.ClearOptions();
            classDropdown.AddOptions(new List<string> { "tech", "miner", "fighter", "comms" });
            classDropdown.value = 0;
            classDropdown.RefreshShownValue();
        }

        if (nicknameInput != null)
            nicknameInput.text = string.Empty;
    }

    public void OnCreate()
    {
        _progress = EnsureProgressService();

        if (nicknameInput == null)
        {
            Debug.LogError("CharacterCreateController.nicknameInput is not assigned.");
            return;
        }

        if (classDropdown == null)
        {
            Debug.LogError("CharacterCreateController.classDropdown is not assigned.");
            return;
        }

        if (classDropdown.options == null || classDropdown.options.Count == 0)
        {
            Debug.LogError("CharacterCreateController.classDropdown has no options.");
            return;
        }

        if (_progress == null)
        {
            Debug.LogError("PlayerProgressService is missing and could not be created.");
            return;
        }

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

    private static PlayerProgressService EnsureProgressService()
    {
        if (PlayerProgressService.Instance != null)
            return PlayerProgressService.Instance;

        PlayerProgressService existing = FindFirstObjectByType<PlayerProgressService>(FindObjectsInactive.Include);
        if (existing != null)
            return existing;

        GameObject go = new GameObject(nameof(PlayerProgressService));
        return go.AddComponent<PlayerProgressService>();
    }
}
