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

        if (_controller == null)
        {
            UnityEngine.Debug.LogError("CharacterSelectController not found in scene.");
            return;
        }

        _controller.EnterCharacterSelect();
    }

    public void Exit() {}
}
