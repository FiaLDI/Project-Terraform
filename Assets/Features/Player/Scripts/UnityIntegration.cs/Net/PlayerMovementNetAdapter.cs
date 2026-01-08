using FishNet.Object;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(MovementStateNetwork))]
    public sealed class PlayerMovementNetAdapter : NetworkBehaviour
    {
        private PlayerMovement movement;
        private PlayerInputState _predictedInput;

        [SerializeField] private float sendRate = 0.05f;
        private float _sendTimer;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            movement = GetComponent<PlayerMovement>();
        }

        // ===== CAMERA =====
        public void SetLookYaw(float yaw)
        {
            if (!IsOwner)
                return;

            _predictedInput.Yaw = yaw;
        }

        // ===== MOVEMENT INPUT =====
        public void ApplyLocalInput(PlayerInputState input)
        {
            if (!IsOwner)
                return;

            // 🔑 КОПИРУЕМ В _predictedInput, А НЕ НАОБОРОТ
            _predictedInput.Move = input.Move;
            _predictedInput.Sprint = input.Sprint;
            _predictedInput.Walk = input.Walk;
            _predictedInput.Jump = input.Jump;
            _predictedInput.Crouch = input.Crouch;

            // ===== LOCAL PREDICTION =====
            movement.SetMoveInput(_predictedInput.Move);
            movement.SetSprint(_predictedInput.Sprint);
            movement.SetWalk(_predictedInput.Walk);

            if (_predictedInput.Crouch)
                movement.ToggleCrouch();

            if (_predictedInput.Jump)
                movement.TryJump();

            movement.SetBodyYaw(_predictedInput.Yaw);

            // ===== SEND TO SERVER =====
            _sendTimer += Time.deltaTime;
            if (_sendTimer >= sendRate)
            {
                _sendTimer = 0f;
                SendInputServerRpc(_predictedInput);
            }
        }

        // ===== SERVER =====
        [ServerRpc]
        private void SendInputServerRpc(PlayerInputState input)
        {
            movement.SetMoveInput(input.Move);
            movement.SetSprint(input.Sprint);
            movement.SetWalk(input.Walk);

            if (input.Crouch)
                movement.ToggleCrouch();

            if (input.Jump)
                movement.TryJump();

            movement.SetBodyYaw(input.Yaw);
        }
    }

}
