using FishNet.Object;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(MovementStateNetwork))]
    public sealed class PlayerMovementNetAdapter : NetworkBehaviour
    {
        private PlayerMovement movement;
        private MovementStateNetwork movementState;

        // ======================================================
        // LIFECYCLE
        // ======================================================

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            movement = GetComponent<PlayerMovement>();
            movementState = GetComponent<MovementStateNetwork>();
            
            Debug.Log($"[PlayerMovementNetAdapter] OnStartNetwork - IsOwner={base.Owner.IsLocalClient}", this);
        }

        private void Update()
        {
            if (!IsServerInitialized || movement == null || movementState == null)
                return;

            Vector3 v = movement.Velocity;
            float planarSpeed = new Vector2(v.x, v.z).magnitude;

            movementState.SetMovementState(
                planarSpeed,
                movement.IsGrounded,
                movement.IsCrouching
            );
        }

        // ======================================================
        // INPUT (CLIENT → LOCAL, NetworkTransform синхронизирует)
        // ======================================================

        public void SendMoveInput(Vector2 input)
        {
            if (!IsOwner)
                return;

            // Client Authoritative: применяем локально, NetworkTransform синхронизирует
            movement.SetMoveInput(input);
        }

        public void SetSprint(bool value)
        {
            if (!IsOwner)
                return;

            movement.SetSprint(value);
        }

        public void SetWalk(bool value)
        {
            if (!IsOwner)
                return;

            movement.SetWalk(value);
        }

        public void ToggleCrouch()
        {
            if (!IsOwner)
                return;

            movement.ToggleCrouch();
        }

        public void Jump()
        {
            if (!IsOwner)
                return;

            movement.TryJump();
        }

        public void SetBodyRotation(float yaw)
        {
            if (!IsOwner)
                return;

            movement.SetBodyYaw(yaw);
        }
    }
}
