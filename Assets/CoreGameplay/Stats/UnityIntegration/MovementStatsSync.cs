using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Features.Stats.Domain;
using Features.Stats.Adapter;
using Features.Stats.Net;

namespace Features.Stats.UnityIntegration
{
    public sealed class MovementStatsSync : NetworkBehaviour
    {
        private readonly SyncVar<MovementStatsSnapshot> synced = new();

        private IStatsFacade stats;
        private StatsFacadeAdapter adapter;

        public override void OnStartServer()
        {
            var owner = GetComponent<IStatsOwner>();
            stats = owner?.Facade;

            if (stats == null)
            {
                Debug.LogError("StatsFacade missing", this);
                return;
            }

            SendSnapshot();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            adapter = GetComponent<StatsFacadeAdapter>();
            synced.OnChange += OnChanged;
            ApplySnapshot(synced.Value);
        }

        public override void OnStopClient()
        {
            synced.OnChange -= OnChanged;
            adapter = null;

            base.OnStopClient();
        }

        private void OnChanged(
            MovementStatsSnapshot oldValue,
            MovementStatsSnapshot newValue,
            bool asServer)
        {
            if (asServer || adapter == null)
                return;

            ApplySnapshot(newValue);
        }

        // вызываем при изменении статов
        public void SendSnapshot()
        {
            if (!IsServer || stats == null)
                return;

            var m = stats.Movement;

            synced.Value = new MovementStatsSnapshot
            {
                walk = m.WalkSpeed,
                sprint = m.SprintSpeed,
                crouch = m.CrouchSpeed,
                rotation = m.RotationSpeed,
                gravity = m.Gravity,
                jumpHeight = m.JumpHeight
            };
        }

        private void ApplySnapshot(MovementStatsSnapshot snapshot)
        {
            if (adapter?.MovementStats == null)
                return;

            adapter.MovementStats.Set(
                snapshot.walk,
                snapshot.sprint,
                snapshot.crouch,
                snapshot.rotation,
                snapshot.gravity,
                snapshot.jumpHeight);
        }
    }
}
