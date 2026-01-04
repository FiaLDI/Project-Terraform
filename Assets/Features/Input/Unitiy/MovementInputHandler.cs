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

        // Actions
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction walkAction;
        private InputAction crouchAction;

        // 🔑 Состояние ввода (батч)
        private PlayerInputState inputState;

        // ======================================================
        // BIND / UNBIND
        // ======================================================

        public void BindInput(PlayerInputContext ctx)
        {
            if (input == ctx)
                return;

            if (input != null)
                UnbindInput(input);

            input = ctx;
            if (input == null)
                return;

            var p = input.Actions.Player;

            moveAction = p.FindAction("Move", true);
            jumpAction = p.FindAction("Jump", true);
            sprintAction = p.FindAction("Sprint", true);
            walkAction = p.FindAction("Walk", true);
            crouchAction = p.FindAction("Crouch", true);

            moveAction.Enable();
            jumpAction.Enable();
            sprintAction.Enable();
            walkAction.Enable();
            crouchAction.Enable();

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
            if (!bound || input != ctx)
                return;

            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMoveCanceled;
            jumpAction.performed -= OnJump;
            sprintAction.performed -= OnSprintStart;
            sprintAction.canceled -= OnSprintStop;
            walkAction.performed -= OnWalkStart;
            walkAction.canceled -= OnWalkStop;
            crouchAction.performed -= OnCrouch;

            input = null;
            bound = false;
        }

        // ======================================================
        // UPDATE (отправка input батчем)
        // ======================================================

        private void Update()
        {
            if (!bound)
                return;

            var movement = GetMovement();
            if (movement == null)
                return;

            // 🔥 единая точка передачи ввода
            movement.ApplyLocalInput(inputState);

            // one-shot флаги
            inputState.Jump = false;
            inputState.Crouch = false;
        }

        // ======================================================
        // INPUT HANDLERS
        // ======================================================

        private PlayerMovementNetAdapter GetMovement()
        {
            var player = LocalPlayerContext.Player;
            return player != null
                ? player.GetComponent<PlayerMovementNetAdapter>()
                : null;
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            inputState.Move = ctx.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext ctx)
        {
            inputState.Move = Vector2.zero;
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            inputState.Jump = true;
        }

        private void OnSprintStart(InputAction.CallbackContext ctx)
        {
            inputState.Sprint = true;
        }

        private void OnSprintStop(InputAction.CallbackContext ctx)
        {
            inputState.Sprint = false;
        }

        private void OnWalkStart(InputAction.CallbackContext ctx)
        {
            inputState.Walk = true;
        }

        private void OnWalkStop(InputAction.CallbackContext ctx)
        {
            inputState.Walk = false;
        }

        private void OnCrouch(InputAction.CallbackContext ctx)
        {
            inputState.Crouch = true;
        }
    }
}
