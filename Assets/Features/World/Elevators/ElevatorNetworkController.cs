using Features.Multiplayer.SceneBinding;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Features.World.Elevators
{
    public sealed class ElevatorNetworkController : SceneBoundNetworkControllerBase
    {
        private readonly SyncVar<int> currentFloor = new();

        private int floorCount = 1;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            currentFloor.OnChange += OnFloorChanged;
        }

        public override void OnStopNetwork()
        {
            currentFloor.OnChange -= OnFloorChanged;
            base.OnStopNetwork();
        }

        protected override void OnServerBoundToView(ISceneBoundView view)
        {
            if (view is ElevatorView elevatorView)
                floorCount = Mathf.Max(1, elevatorView.FloorCount);
        }

        protected override void ServerHandleInteraction(
            SceneBoundInteractionCommand command,
            NetworkConnection sender)
        {
            switch (command)
            {
                case SceneBoundInteractionCommand.Primary:
                case SceneBoundInteractionCommand.Next:
                    currentFloor.Value = (currentFloor.Value + 1) % floorCount;
                    break;

                case SceneBoundInteractionCommand.Previous:
                    currentFloor.Value--;
                    if (currentFloor.Value < 0)
                        currentFloor.Value = floorCount - 1;
                    break;
            }
        }

        private void OnFloorChanged(int prev, int next, bool asServer)
        {
            ReapplyStateToView(false);
        }

        protected override void OnApplyStateToView(ISceneBoundView view, bool snap)
        {
            if (view is ElevatorView elevatorView)
                elevatorView.SetFloor(currentFloor.Value, snap);
        }
    }
}
