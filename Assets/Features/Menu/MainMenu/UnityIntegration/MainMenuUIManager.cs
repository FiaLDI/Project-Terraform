using UnityEngine;
using System.Collections.Generic;

public class MainMenuUIManager : MonoBehaviour
{
    public static MainMenuUIManager Instance { get; private set; }

    public GameObject playPanel;
    public GameObject modeSelectPanel;
    public GameObject characterSelectPanel;
    public GameObject characterCreatePanel;
    public GameObject multiplayerPlaceholderPanel;

    private Dictionary<MainMenuStateId, GameObject> _panels;

    private void Awake()
    {
        Instance = this;

        _panels = new Dictionary<MainMenuStateId, GameObject>
        {
            { MainMenuStateId.Play, playPanel },
            { MainMenuStateId.ModeSelect, modeSelectPanel },
            { MainMenuStateId.CharacterSelect, characterSelectPanel },
            { MainMenuStateId.CharacterCreate, characterCreatePanel },
            { MainMenuStateId.MultiplayerPlaceholder, multiplayerPlaceholderPanel }
        };
    }

    public void Show(MainMenuStateId state)
    {
        foreach (var p in _panels)
            p.Value.SetActive(p.Key == state);
    }
}
