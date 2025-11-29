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

    public bool SettingsMenuOpen => settingsMenu != null && settingsMenu.IsOpen;

    private SettingsCaller lastCaller = SettingsCaller.None;

    private void Awake()
    {
        I = this;
        settingsMenu.HideInstant();
    }

    // --- Вызов настроек ---
    public void OpenSettings(SettingsCaller caller)
    {
        lastCaller = caller;
        settingsMenu.Show();
    }

    // --- Закрытие настроек ---
    public void CloseSettings()
    {
        settingsMenu.HideInstant();

        switch (lastCaller)
        {
            case SettingsCaller.PauseMenu:
                pauseMenu.Open();
                break;

            case SettingsCaller.Gameplay:
                Time.timeScale = 1;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case SettingsCaller.StationUI:
                break;

            case SettingsCaller.MainMenu:
                break;
        }

        lastCaller = SettingsCaller.None;
    }
}
