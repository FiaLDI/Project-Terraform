using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Screens")]
    public GameObject modeSelectScreen;
    public GameObject settingsScreen;
    public GameObject mainMenuScreen;

    private void Start()
    {
        ShowMain();
    }

    public void ShowMain()
    {
        mainMenuScreen.SetActive(true);
        modeSelectScreen.SetActive(false);
        settingsScreen.SetActive(false);
    }

    public void OnPlayPressed()
    {
        mainMenuScreen.SetActive(false);
        modeSelectScreen.SetActive(true);
    }

    public void OnSettingsPressed()
    {
        mainMenuScreen.SetActive(false);
        settingsScreen.SetActive(true);
    }

    public void OnExitPressed()
    {
        Application.Quit();
    }

    public void OnSettingsBack()
    {
        ShowMain();
    }
}
