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

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            movementState = GetComponent<MovementStateNetwork>();
        }

        // ======================================================
        // LIFECYCLE
        // ======================================================

        public override void OnStartServer()
        {
            base.OnStartServer();
            movement.AllowMovement = true;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            movement.AllowMovement = false;
        }

        private void Update()
        {
            // 🔑 ТОЛЬКО сервер публикует состояние
            if (!IsServerInitialized)
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
        // INPUT (CLIENT → SERVER)
        // ======================================================

        public void SendMoveInput(Vector2 input)
        {
            if (!IsOwner)
                return;

            if (IsServerInitialized)
                movement.SetMoveInput(input);
            else
                SendMoveInput_Server(input);
        }

        [ServerRpc]
        private void SendMoveInput_Server(Vector2 input)
        {
            movement.SetMoveInput(input);
        }

        public void SetSprint(bool value)
        {
            if (!IsOwner)
                return;

            if (IsServerInitialized)
                movement.SetSprint(value);
            else
                SetSprint_Server(value);
        }

        [ServerRpc]
        private void SetSprint_Server(bool value)
        {
            movement.SetSprint(value);
        }

        public void SetWalk(bool value)
        {
            if (!IsOwner)
                return;

            if (IsServerInitialized)
                movement.SetWalk(value);
            else
                SetWalk_Server(value);
        }

        [ServerRpc]
        private void SetWalk_Server(bool value)
        {
            movement.SetWalk(value);
        }

        public void ToggleCrouch()
        {
            if (!IsOwner)
                return;

            if (IsServerInitialized)
                movement.ToggleCrouch();
            else
                ToggleCrouch_Server();
        }

        [ServerRpc]
        private void ToggleCrouch_Server()
        {
            movement.ToggleCrouch();
        }

        public void Jump()
        {
            if (!IsOwner)
                return;

            if (IsServerInitialized)
                movement.TryJump();
            else
                Jump_Server();
        }

        [ServerRpc]
        private void Jump_Server()
        {
            movement.TryJump();
        }

        public void SetBodyRotation(float yaw)
        {
            if (!IsOwner)
                return;

            if (IsServerInitialized)
                movement.SetBodyYaw(yaw);
            else
                SetBodyRotation_Server(yaw);
        }

        [ServerRpc]
        private void SetBodyRotation_Server(float yaw)
        {
            movement.SetBodyYaw(yaw);
        }
    }
}
