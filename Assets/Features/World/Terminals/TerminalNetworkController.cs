using Features.Multiplayer.SceneBinding;
using FishNet.Connection;
using FishNet.Object.Synchronizing;

namespace Features.World.Terminals
{
    public sealed class TerminalNetworkController : SceneBoundNetworkControllerBase
    {
        private readonly SyncVar<bool> powered = new();
        private readonly SyncVar<bool> busy = new();

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            powered.OnChange += OnStateChanged;
            busy.OnChange += OnStateChanged;
        }

        public override void OnStopNetwork()
        {
            powered.OnChange -= OnStateChanged;
            busy.OnChange -= OnStateChanged;
            base.OnStopNetwork();
        }

        protected override void OnServerBoundToView(ISceneBoundView view)
        {
            powered.Value = true;
        }

        protected override void ServerHandleInteraction(
            SceneBoundInteractionCommand command,
            NetworkConnection sender)
        {
            if (command is not SceneBoundInteractionCommand.Primary
                and not SceneBoundInteractionCommand.Use)
                return;

            if (!powered.Value)
                return;

            busy.Value = !busy.Value;
        }

        private void OnStateChanged(bool prev, bool next, bool asServer)
        {
            ReapplyStateToView(false);
        }

        protected override void OnApplyStateToView(ISceneBoundView view, bool snap)
        {
            if (view is TerminalView terminalView)
                terminalView.SetState(powered.Value, busy.Value);
        }
    }
}
