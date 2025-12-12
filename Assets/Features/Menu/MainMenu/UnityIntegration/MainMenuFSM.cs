using UnityEngine;
using System.Collections.Generic;

public class MainMenuFSM : MonoBehaviour
{
    public static MainMenuFSM Instance { get; private set; }

    private IMainMenuState _current;
    private MainMenuStateId _currentStateId;

    private Dictionary<MainMenuStateId, IMainMenuState> _states;

    private void Awake()
    {
        Instance = this;
    }

    public void Init(Dictionary<MainMenuStateId, IMainMenuState> states)
    {
        _states = states;
    }

    public void Switch(MainMenuStateId id)
    {
        _current?.Exit();
        _current = _states[id];
        _currentStateId = id;
        _current.Enter();
    }

    public MainMenuStateId GetBackState(MainMenuStateId current)
    {
        return current switch
        {
            MainMenuStateId.Play => MainMenuStateId.Play, // уже на корне
            MainMenuStateId.ModeSelect => MainMenuStateId.Play,
            MainMenuStateId.CharacterSelect => MainMenuStateId.ModeSelect,
            MainMenuStateId.CharacterCreate => MainMenuStateId.CharacterSelect,
            MainMenuStateId.MultiplayerPlaceholder => MainMenuStateId.ModeSelect,
            MainMenuStateId.Settings => MainMenuStateId.Play, // выход из настроек в главное меню
            _ => MainMenuStateId.Play
        };
    }

    public void Back()
    {
        var backState = GetBackState(_currentStateId);
        Switch(backState);
    }
}

public enum MainMenuStateId
{
    Play,
    ModeSelect,
    CharacterSelect,
    CharacterCreate,
    MultiplayerPlaceholder,
    Settings
}
