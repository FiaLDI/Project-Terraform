public class PlayMenuState : IMainMenuState
{
    public void Enter()
    {
        MainMenuUIManager.Instance.Show(MainMenuStateId.Play);
    }

    public void Exit()
    {
        MainMenuUIManager.Instance.playPanel.SetActive(false);
    }
}
