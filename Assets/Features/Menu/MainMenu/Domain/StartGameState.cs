public sealed class StartGameState : IMainMenuState
{
    public void Enter()
    {
        MainMenuUIManager.Instance.Show(MainMenuStateId.StartGame);
    }

    public void Exit()
    {
    }
}