using UnityEngine;
using UnityEngine.InputSystem;
using Features.Input;
using Features.Player;

namespace Features.Player.UnityIntegration
{
    public sealed class MovementInputHandler : MonoBehaviour, IInputContextConsumer
    {
        private PlayerInputContext input;
        private bool bound;

        // Кэшируем делегаты, чтобы не мусорить
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction walkAction;
        private InputAction crouchAction;

        public void BindInput(PlayerInputContext ctx)
        {
            if (input == ctx) return;
            if (input != null) UnbindInput(input);

            input = ctx;
            if (input == null) return;

            var p = input.Actions.Player;
            Enable(p, "Move", "Jump", "Sprint", "Walk", "Crouch");

            moveAction = p.FindAction("Move", true);
            jumpAction = p.FindAction("Jump", true);
            sprintAction = p.FindAction("Sprint", true);
            walkAction = p.FindAction("Walk", true);
            crouchAction = p.FindAction("Crouch", true);

            moveAction.performed += OnMove;
            moveAction.canceled += OnMoveCanceled;
            jumpAction.performed += OnJump;
            sprintAction.performed += OnSprintStart;
            sprintAction.canceled += OnSprintStop;
            walkAction.performed += OnWalkStart;
            walkAction.canceled += OnWalkStop;
            crouchAction.performed += OnCrouch;

            bound = true;
        }

        public void UnbindInput(PlayerInputContext ctx)
        {
            if (!bound || input != ctx) return;

            if (moveAction != null) { moveAction.performed -= OnMove; moveAction.canceled -= OnMoveCanceled; }
            if (jumpAction != null) jumpAction.performed -= OnJump;
            if (sprintAction != null) { sprintAction.performed -= OnSprintStart; sprintAction.canceled -= OnSprintStop; }
            if (walkAction != null) { walkAction.performed -= OnWalkStart; walkAction.canceled -= OnWalkStop; }
            if (crouchAction != null) crouchAction.performed -= OnCrouch;

            input = null;
            bound = false;
        }

        // --- HANDLERS ---

        private PlayerMovementNetAdapter GetMovement()
        {
            var player = LocalPlayerContext.Player;
            return player != null ? player.GetComponent<PlayerMovementNetAdapter>() : null;
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            GetMovement()?.SendMoveInput(ctx.ReadValue<Vector2>());
        }

        private void OnMoveCanceled(InputAction.CallbackContext ctx)
        {
            GetMovement()?.SendMoveInput(Vector2.zero);
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            GetMovement()?.Jump();
        }

        private void OnSprintStart(InputAction.CallbackContext ctx) => GetMovement()?.SetSprint(true);
        private void OnSprintStop(InputAction.CallbackContext ctx) => GetMovement()?.SetSprint(false);
        private void OnWalkStart(InputAction.CallbackContext ctx) => GetMovement()?.SetWalk(true);
        private void OnWalkStop(InputAction.CallbackContext ctx) => GetMovement()?.SetWalk(false);
        private void OnCrouch(InputAction.CallbackContext ctx) => GetMovement()?.ToggleCrouch();

        private static void Enable(InputActionMap map, params string[] names)
        {
            foreach (var n in names) map.FindAction(n, true).Enable();
        }
    }
}
