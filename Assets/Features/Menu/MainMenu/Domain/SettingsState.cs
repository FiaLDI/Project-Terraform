public class SettingsState : IMainMenuState
{
    public void Enter()
    {
        MainMenuUIManager.Instance.Show(MainMenuStateId.Settings);
    }

    public void Exit()
    {
        // Ничего
    }
}
