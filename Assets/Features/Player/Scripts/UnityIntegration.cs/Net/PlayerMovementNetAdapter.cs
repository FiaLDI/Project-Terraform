using FishNet.Object;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    /// <summary>
    /// Server-authoritative adapter:
    /// - Клиент отправляет input
    /// - Сервер двигает PlayerMovement
    /// - Host обрабатывается корректно
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public sealed class PlayerMovementNetAdapter : NetworkBehaviour
    {
        private PlayerMovement movement;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();

            Debug.Log(
                $"[MoveNet][Awake] {name} | " +
                $"netId={ObjectId}"
            );
        }

        // ======================================================
        // LIFECYCLE
        // ======================================================

        public override void OnStartServer()
        {
            base.OnStartServer();

            Debug.Log(
                $"[MoveNet][OnStartServer] {name} | " +
                $"IsServer={IsServerInitialized} IsOwner=NOTAALOWED"
            );

            movement.AllowMovement = true;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            movement.AllowMovement = false;
        }

        // ======================================================
        // MOVEMENT INPUT (CLIENT → SERVER)
        // ======================================================

        public void SendMoveInput(Vector2 input)
        {
            Debug.Log(
                $"[MoveNet][Input] {name} | " +
                $"input={input} IsOwner={IsOwner} IsServer={IsServerInitialized}"
            );

            if (!IsOwner)
            {
                Debug.Log($"[MoveNet][Input BLOCKED] not owner: {name}");
                return;
            }

            if (IsServerInitialized)
            {
                Debug.Log($"[MoveNet][HOST MOVE] {name}");
                movement.SetMoveInput(input);
            }
            else
            {
                Debug.Log($"[MoveNet][CLIENT → SERVER RPC] {name}");
                SendMoveInput_Server(input);
            }
        }

        [ServerRpc]
        private void SendMoveInput_Server(Vector2 input)
        {
            Debug.Log(
                $"[MoveNet][ServerRpc] {name} | " +
                $"input={input} IsServer={IsServerInitialized}"
            );

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

        // ======================================================
        // BODY ROTATION (FROM CAMERA)
        // ======================================================

        /// <summary>
        /// Поворот тела игрока (Yaw).
        /// Вызывается ТОЛЬКО из локальной камеры.
        /// </summary>
        public void SetBodyRotation(float yaw)
        {
            if (!IsOwner)
                return;

            if (IsServerInitialized)
            {
                movement.SetBodyYaw(yaw);
            }
            else
            {
                SetBodyRotation_Server(yaw);
            }
        }

        [ServerRpc]
        private void SetBodyRotation_Server(float yaw)
        {
            movement.SetBodyYaw(yaw);
        }
    }
}
