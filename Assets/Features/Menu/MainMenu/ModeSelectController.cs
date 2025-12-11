using UnityEngine;

public class ModeSelectController : MonoBehaviour
{
    [Header("Screens")]
    public GameObject modeSelectScreen;
    public GameObject characterSelectScreen;
    public GameObject multiplayerPlaceholderScreen;
    public GameObject mainMenuScreen;

    public void OnSingleplayerPressed()
    {
        modeSelectScreen.SetActive(false);
        characterSelectScreen.SetActive(true);
    }

    public void OnMultiplayerPressed()
    {
        // Заглушка "В разработке"
        modeSelectScreen.SetActive(false);
        multiplayerPlaceholderScreen.SetActive(true);
    }

    public void OnBackPressed()
    {
        modeSelectScreen.SetActive(false);
        mainMenuScreen.SetActive(true);
    }

    public void OnMultiplayerPlaceholderBack()
    {
        multiplayerPlaceholderScreen.SetActive(false);
        modeSelectScreen.SetActive(true);
    }
}
