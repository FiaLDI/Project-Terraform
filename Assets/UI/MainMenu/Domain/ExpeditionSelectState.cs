public sealed class ExpeditionSelectState : IMainMenuState
{
    private readonly ExpeditionSelectController _controller;

    public ExpeditionSelectState(ExpeditionSelectController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        MainMenuUIManager.Instance.Show(MainMenuStateId.ExpeditionSelect);

        if (_controller == null)
        {
            UnityEngine.Debug.LogError("ExpeditionSelectController not found in scene.");
            return;
        }

        _controller.EnterExpeditionSelect();
    }

    public void Exit()
    {
    }
}
