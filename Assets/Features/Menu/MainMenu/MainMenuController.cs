using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    private void Start()
    {
        var input = UnityEngine.Object.FindFirstObjectByType<PlayerInput>();
        if (input != null)
            input.SwitchCurrentActionMap("UI");
        var fsm = MainMenuFSM.Instance;
        var controller = Object.FindFirstObjectByType<CharacterSelectController>();

        fsm.Init(new Dictionary<MainMenuStateId, IMainMenuState>
        {
            { MainMenuStateId.Play, new PlayMenuState() },
            { MainMenuStateId.ModeSelect, new ModeSelectState() },
            { MainMenuStateId.CharacterSelect, new CharacterSelectState(controller) },
            { MainMenuStateId.CharacterCreate, new CharacterCreateState() },
            { MainMenuStateId.MultiplayerPlaceholder,
                new MultiplayerPlaceholderState() },
            { MainMenuStateId.Settings, new SettingsState() } 
        });

        fsm.Switch(MainMenuStateId.Play);
    }

    public void OnPlayPressed() => MainMenuFSM.Instance.Switch(MainMenuStateId.ModeSelect);
    public void OnSettingsPressed()
    {
        SettingsMenuManager.I.OpenSettings(SettingsCaller.MainMenu);
    }
    public void OnExitPressed() => Application.Quit();
}
