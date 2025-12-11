using UnityEngine;
using System.Collections.Generic;

public class CharacterSelectController : MonoBehaviour
{
    public Transform characterListRoot;
    public CharacterCardView characterCardPrefab;
    public UnityEngine.UI.Button playButton;
    public UnityEngine.UI.Button deleteButton;

    private PlayerProgressService _progress;
    private PlayerProfile _profile;

    private List<CharacterCardView> _cards = new();
    private int _selectedIndex = -1;

    private void Start()
    {
        _progress = PlayerProgressService.Instance;
        _profile = _progress.Data.profile;
    }

    public void RefreshList()
    {
        foreach (var c in _cards)
            Destroy(c.gameObject);
        _cards.Clear();

        for (int i = 0; i < _profile.characters.Count; i++)
        {
            var card = Instantiate(characterCardPrefab, characterListRoot);
            card.Setup(_profile.characters[i], i, SelectCharacter);
            _cards.Add(card);
        }

        UpdateButtons();
    }

    private void SelectCharacter(int index)
    {
        _selectedIndex = index;
        _progress.SelectCharacter(index);
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        bool valid = _selectedIndex >= 0;
        playButton.interactable = valid;
        deleteButton.interactable = valid;
    }

    public void OnCreateNew()
        => MainMenuFSM.Instance.Switch(MainMenuStateId.CharacterCreate);

    public void OnBack()
        => MainMenuFSM.Instance.Switch(MainMenuStateId.ModeSelect);

    public void OnPlay()
        => UnityEngine.SceneManagement.SceneManager.LoadScene("HubScene");

    public void OnDelete()
    {
        if (_selectedIndex < 0) return;

        _progress.DeleteCharacter(_selectedIndex);
        _selectedIndex = -1;
        RefreshList();
    }
}
