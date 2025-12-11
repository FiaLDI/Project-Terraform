using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class CharacterCreateController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown classDropdown;
    public TMP_InputField nicknameInput;

    [Header("Screens")]
    public CharacterSelectController characterSelectController;
    public GameObject characterSelectScreen;
    public GameObject characterCreateScreen;

    private PlayerProgressService _progress;

    private void OnEnable()
    {
        _progress = PlayerProgressService.Instance;

        classDropdown.ClearOptions();
        classDropdown.AddOptions(new List<string> { "engineer", "miner", "fighter", "comms" });
    }

    public void OnCreatePressed()
    {
        string nickname = nicknameInput.text.Trim();
        string classId = classDropdown.options[classDropdown.value].text;

        if (nickname.Length == 0)
        {
            Debug.LogWarning("Nickname cannot be empty!");
            return;
        }

        _progress.AddCharacter(classId, nickname);

        characterCreateScreen.SetActive(false);
        characterSelectScreen.SetActive(true);

        characterSelectController.RefreshList();
    }

    public void OnCancelPressed()
    {
        characterCreateScreen.SetActive(false);
        characterSelectScreen.SetActive(true);
    }
}
