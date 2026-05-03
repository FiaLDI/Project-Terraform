using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MainMenuUIManager : MonoBehaviour
{
    public static MainMenuUIManager Instance { get; private set; }

    public GameObject playPanel;
    public GameObject characterSelectPanel;
    public GameObject characterCreatePanel;
    public GameObject expeditionSelectPanel;
    public GameObject expeditionCreatePanel;
    public GameObject startGamePanel;
    public GameObject SettingsMenuPanel;

    private Dictionary<MainMenuStateId, GameObject> _panels;

    private void Awake()
    {
        Instance = this;

        _panels = new Dictionary<MainMenuStateId, GameObject>
        {
            { MainMenuStateId.Play, playPanel },
            { MainMenuStateId.CharacterSelect, characterSelectPanel },
            { MainMenuStateId.CharacterCreate, characterCreatePanel },
            { MainMenuStateId.ExpeditionSelect, expeditionSelectPanel },
            { MainMenuStateId.ExpeditionCreate, expeditionCreatePanel },
            { MainMenuStateId.StartGame, startGamePanel },
            { MainMenuStateId.Settings, SettingsMenuPanel }
        };
    }

    public void Show(MainMenuStateId state)
    {
        if (!_panels.ContainsKey(state))
        {
            Debug.LogError($"Panel for state {state} not registered!");
            return;
        }

        GameObject activePanel = _panels[state];

        foreach (GameObject panel in _panels.Values.Where(x => x != null).Distinct())
            panel.SetActive(panel == activePanel);
    }
}
