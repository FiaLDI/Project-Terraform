using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour, IUIScreen
{
    public InputMode Mode => InputMode.Pause;

    private void Start()
    {
        LoadingScreenService.Hide();

        var fsm = MainMenuFSM.Instance;
        var controller = Object.FindFirstObjectByType<CharacterSelectController>();

        fsm.Init(new Dictionary<MainMenuStateId, IMainMenuState>
        {
            { MainMenuStateId.Play, new PlayMenuState() },
            { MainMenuStateId.CharacterSelect, new CharacterSelectState(controller) },
            { MainMenuStateId.CharacterCreate, new CharacterCreateState() },
            { MainMenuStateId.StartGame, new StartGameState() },
            { MainMenuStateId.Settings, new SettingsState() }
        });

        fsm.Switch(MainMenuStateId.Play);
        UIStackManager.I.Push(this);
    }

    public void OnPlayPressed()
    {
        MainMenuFSM.Instance.Switch(MainMenuStateId.CharacterSelect);
    }

    public void OnSettingsPressed()
    {
        MainMenuFSM.Instance.Switch(MainMenuStateId.Settings);
        SettingsMenu.I.Open();
    }

    public void OnExitPressed()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        UIStackManager.I.Clear();
    }

    public void Show()
    {
        
    }

    public void Hide()
    {
        
    }
}
