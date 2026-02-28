public class CharacterSelectState : IMainMenuState
{
    private CharacterSelectController _controller;

    public CharacterSelectState(CharacterSelectController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        MainMenuUIManager.Instance.Show(MainMenuStateId.CharacterSelect);
        _controller.RefreshList();
    }

    public void Exit() {}
}
