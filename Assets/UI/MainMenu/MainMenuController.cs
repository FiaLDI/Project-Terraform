using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour, IUIScreen
{
    public InputMode Mode => InputMode.Pause;

    private void Start()
    {
        LoadingScreenService.Hide();

        var fsm = MainMenuFSM.Instance;
        var characterSelectController = GetRequiredController<CharacterSelectController>();
        var characterCreateController = GetRequiredController<CharacterCreateController>();
        var expeditionSelectController = GetRequiredController<ExpeditionSelectController>();
        var expeditionCreateController = GetRequiredController<ExpeditionCreateController>();

        fsm.Init(new Dictionary<MainMenuStateId, IMainMenuState>
        {
            { MainMenuStateId.Play, new PlayMenuState() },
            { MainMenuStateId.CharacterSelect, new CharacterSelectState(characterSelectController) },
            { MainMenuStateId.CharacterCreate, new CharacterCreateState(characterCreateController) },
            { MainMenuStateId.ExpeditionSelect, new ExpeditionSelectState(expeditionSelectController) },
            { MainMenuStateId.ExpeditionCreate, new ExpeditionCreateState(expeditionCreateController) },
            { MainMenuStateId.StartGame, new StartGameState() },
            { MainMenuStateId.Settings, new SettingsState() }
        });

        fsm.Switch(MainMenuStateId.Play);

        if (UIStackManager.I != null)
            UIStackManager.I.Push(this);
        else
            Debug.LogWarning("UIStackManager is missing. Main menu will work without stack registration.");
    }

    private T GetRequiredController<T>() where T : Component
    {
        T controller = GetComponent<T>();
        if (controller == null)
            Debug.LogError($"{typeof(T).Name} is missing on '{name}'.");

        return controller;
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
        UIStackManager.I?.Clear();
    }

    public void Show()
    {
        
    }

    public void Hide()
    {
        
    }
}
