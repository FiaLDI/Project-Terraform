using Features.Multiplayer.SceneBinding;
using FishNet.Connection;
using FishNet.Object.Synchronizing;

namespace Features.World.Containers
{
    public sealed class ChestNetworkController : SceneBoundNetworkControllerBase
    {
        private readonly SyncVar<bool> isOpen = new();
        private readonly SyncVar<bool> isLocked = new();

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            isOpen.OnChange += OnOpenChanged;
        }

        public override void OnStopNetwork()
        {
            isOpen.OnChange -= OnOpenChanged;
            base.OnStopNetwork();
        }

        protected override void ServerHandleInteraction(
            SceneBoundInteractionCommand command,
            NetworkConnection sender)
        {
            if (command is not SceneBoundInteractionCommand.Primary
                and not SceneBoundInteractionCommand.Open
                and not SceneBoundInteractionCommand.Use)
                return;

            if (isLocked.Value)
                return;

            isOpen.Value = true;
        }

        private void OnOpenChanged(bool prev, bool next, bool asServer)
        {
            ReapplyStateToView(false);
        }

        protected override void OnApplyStateToView(ISceneBoundView view, bool snap)
        {
            if (view is ChestView chestView)
                chestView.SetOpen(isOpen.Value, snap);
        }
    }
}
