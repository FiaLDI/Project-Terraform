using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectController : MonoBehaviour
{
    public Transform characterListRoot;
    public CharacterCardView characterCardPrefab;
    public Button playButton;
    public Button deleteButton;

    private PlayerProgressService _progress;
    private PlayerProfile _profile;

    private readonly List<CharacterCardView> _cards = new();
    private int _selectedIndex = -1;

    private void Start()
    {
        _progress = PlayerProgressService.Instance;
        _profile = _progress != null && _progress.Data != null
            ? _progress.Data.profile
            : null;
    }

    public void EnterCharacterSelect()
    {
        RefreshList();
    }

    public void RefreshList()
    {
        if (_progress == null)
        {
            _progress = PlayerProgressService.Instance;
            _profile = _progress != null && _progress.Data != null
                ? _progress.Data.profile
                : null;
        }

        foreach (CharacterCardView card in _cards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        _cards.Clear();
        _selectedIndex = _profile != null ? _profile.activeCharacterIndex : -1;

        if (_profile != null && _profile.characters != null)
        {
            for (int i = 0; i < _profile.characters.Count; i++)
            {
                CharacterCardView card = Instantiate(characterCardPrefab, characterListRoot);
                card.Setup(_profile.characters[i], i, SelectCharacter, _selectedIndex);
                _cards.Add(card);
            }
        }

        UpdateButtons();
        UpdateCardSelection();
    }

    public void OnCreateNew()
    {
        MainMenuFSM.Instance.Switch(MainMenuStateId.CharacterCreate);
    }

    public void OnBack()
    {
        MainMenuFSM.Instance.Switch(MainMenuStateId.Play);
    }

    public void OnPlay()
    {
        if (_selectedIndex < 0)
            return;

        MainMenuFSM.Instance.Switch(MainMenuStateId.ExpeditionSelect);
    }

    public void OnDelete()
    {
        if (_selectedIndex < 0)
            return;

        _progress.DeleteCharacter(_selectedIndex);
        _selectedIndex = -1;
        RefreshList();
    }

    private void SelectCharacter(int index)
    {
        _selectedIndex = index;
        _progress.SelectCharacter(index);
        UpdateButtons();
        UpdateCardSelection();
    }

    private void UpdateButtons()
    {
        bool valid = _selectedIndex >= 0;

        if (playButton != null)
            playButton.interactable = valid;

        if (deleteButton != null)
            deleteButton.interactable = valid;
    }

    private void UpdateCardSelection()
    {
        for (int i = 0; i < _cards.Count; i++)
            _cards[i].SetSelected(i == _selectedIndex);
    }
}
