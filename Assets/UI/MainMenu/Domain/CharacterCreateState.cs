public class CharacterCreateState : IMainMenuState
{
    private readonly CharacterCreateController _controller;

    public CharacterCreateState(CharacterCreateController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        MainMenuUIManager.Instance.Show(MainMenuStateId.CharacterCreate);

        if (_controller == null)
        {
            UnityEngine.Debug.LogError("CharacterCreateController not found in scene.");
            return;
        }

        _controller.EnterCharacterCreate();
    }

    public void Exit() {}
}
