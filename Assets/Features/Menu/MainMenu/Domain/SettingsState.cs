public class SettingsState : IMainMenuState
{
    public void Enter()
    {
        // НЕ трогаем UIManager
        SettingsMenuManager.I.OpenSettings(SettingsCaller.MainMenu);
    }

    public void Exit()
    {
        // Ничего
    }
}
