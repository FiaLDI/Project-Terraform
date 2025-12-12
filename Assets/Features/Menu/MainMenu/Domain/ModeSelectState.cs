public class ModeSelectState : IMainMenuState
{
    public void Enter()
    {
        MainMenuUIManager.Instance.Show(MainMenuStateId.ModeSelect);
    }

    public void Exit() {}
}
