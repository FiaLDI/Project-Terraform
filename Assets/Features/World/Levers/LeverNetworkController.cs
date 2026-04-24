using Features.Multiplayer.SceneBinding;
using FishNet.Connection;
using FishNet.Object.Synchronizing;

namespace Features.World.Levers
{
    public sealed class LeverNetworkController : SceneBoundNetworkControllerBase
    {
        private readonly SyncVar<bool> isOn = new();

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            isOn.OnChange += OnStateChanged;
        }

        public override void OnStopNetwork()
        {
            isOn.OnChange -= OnStateChanged;
            base.OnStopNetwork();
        }

        protected override void ServerHandleInteraction(
            SceneBoundInteractionCommand command,
            NetworkConnection sender)
        {
            if (command is not SceneBoundInteractionCommand.Primary
                and not SceneBoundInteractionCommand.Toggle
                and not SceneBoundInteractionCommand.Use)
                return;

            isOn.Value = !isOn.Value;
        }

        private void OnStateChanged(bool prev, bool next, bool asServer)
        {
            ReapplyStateToView(false);
        }

        protected override void OnApplyStateToView(ISceneBoundView view, bool snap)
        {
            if (view is LeverView leverView)
                leverView.SetOn(isOn.Value, snap);
        }
    }
}
