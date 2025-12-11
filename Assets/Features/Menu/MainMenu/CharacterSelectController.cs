using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterSelectController : MonoBehaviour
{
    [Header("UI References")]
    public Transform characterListRoot;
    public CharacterCardView characterCardPrefab;

    public Button playButton;
    public Button deleteButton;

    [Header("Screens")]
    public GameObject characterSelectScreen;
    public GameObject characterCreateScreen;
    public GameObject modeSelectScreen;

    private PlayerProgressService _progress;
    private PlayerProfile _profile;

    private int _selectedIndex = -1;
    private List<CharacterCardView> _spawnedCards = new List<CharacterCardView>();

    private void OnEnable()
    {
        _progress = PlayerProgressService.Instance;
        _profile = _progress.Data.profile;

        RefreshList();
    }

    public void RefreshList()
    {
        foreach (var card in _spawnedCards)
            Destroy(card.gameObject);

        _spawnedCards.Clear();

        var chars = _profile.characters;

        for (int i = 0; i < chars.Count; i++)
        {
            var card = Instantiate(characterCardPrefab, characterListRoot);
            card.Setup(chars[i], i, OnCharacterSelected);
            _spawnedCards.Add(card);
        }

        UpdateButtons();
    }

    private void OnCharacterSelected(int index)
    {
        _selectedIndex = index;

        _progress.SelectCharacter(index);

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        bool valid = _selectedIndex >= 0 && _selectedIndex < _profile.characters.Count;
        playButton.interactable = valid;
        deleteButton.interactable = valid;
    }

    public void OnCreateNewPressed()
    {
        characterSelectScreen.SetActive(false);
        characterCreateScreen.SetActive(true);
    }

    public void OnDeletePressed()
    {
        if (_selectedIndex < 0) return;

        _progress.DeleteCharacter(_selectedIndex);

        _selectedIndex = -1;
        RefreshList();
    }

    public void OnPlayPressed()
    {
        if (_selectedIndex < 0) return;

        UnityEngine.SceneManagement.SceneManager.LoadScene("HubScene");
    }

    public void OnBackPressed()
    {
        characterSelectScreen.SetActive(false);
        modeSelectScreen.SetActive(true);
    }
}
