using UnityEngine;

public enum SettingsCaller
{
    None,
    PauseMenu,
    MainMenu,
    Gameplay,
    StationUI
}

public class SettingsMenuManager : MonoBehaviour
{
    public static SettingsMenuManager I;

    [Header("References")]
    public SettingsMenu settingsMenu;
    public PauseMenu pauseMenu;

    private SettingsCaller lastCaller = SettingsCaller.None;

    private void Awake()
    {
        I = this;
        settingsMenu.HideInstant();
    }

    public void OpenSettings(SettingsCaller caller)
    {
        lastCaller = caller;
        settingsMenu.Show();
    }

    public void CloseSettings()
    {
        switch (lastCaller)
        {
            case SettingsCaller.PauseMenu:
                pauseMenu?.Open();
                break;

            case SettingsCaller.Gameplay:
                Time.timeScale = 1;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case SettingsCaller.MainMenu:
            case SettingsCaller.StationUI:
                break;
        }

        lastCaller = SettingsCaller.None;
    }
}
