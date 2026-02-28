using UnityEngine;
using System.Collections.Generic;

public class MainMenuUIManager : MonoBehaviour
{
    public static MainMenuUIManager Instance { get; private set; }

    public GameObject playPanel;
    public GameObject characterSelectPanel;
    public GameObject characterCreatePanel;
    public GameObject startGamePanel;

    private Dictionary<MainMenuStateId, GameObject> _panels;

    private void Awake()
    {
        Instance = this;

        _panels = new Dictionary<MainMenuStateId, GameObject>
        {
            { MainMenuStateId.Play, playPanel },
            { MainMenuStateId.CharacterSelect, characterSelectPanel },
            { MainMenuStateId.CharacterCreate, characterCreatePanel },
            { MainMenuStateId.StartGame, startGamePanel },
        };
    }

    public void Show(MainMenuStateId state)
    {
        if (!_panels.ContainsKey(state))
        {
            Debug.LogError($"Panel for state {state} not registered!");
            return;
        }

        foreach (var p in _panels)
            p.Value.SetActive(p.Key == state);
    }
}
