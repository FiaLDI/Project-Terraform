public class MultiplayerPlaceholderState : IMainMenuState
{
    public void Enter()
    {
        MainMenuUIManager.Instance.Show(MainMenuStateId.MultiplayerPlaceholder);
    }

    public void Exit() {}
}
