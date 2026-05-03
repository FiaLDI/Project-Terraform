public sealed class ExpeditionCreateState : IMainMenuState
{
    private readonly ExpeditionCreateController _controller;

    public ExpeditionCreateState(ExpeditionCreateController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        MainMenuUIManager.Instance.Show(MainMenuStateId.ExpeditionCreate);

        if (_controller == null)
        {
            UnityEngine.Debug.LogError("ExpeditionCreateController not found in scene.");
            return;
        }

        _controller.EnterExpeditionCreate();
    }

    public void Exit()
    {
    }
}
