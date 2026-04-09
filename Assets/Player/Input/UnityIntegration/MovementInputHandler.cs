using UnityEngine;
using UnityEngine.InputSystem;
using Features.Input;

namespace Features.Player.UnityIntegration
{
    /// <summary>
    /// Собирает локальный input.
    /// НИЧЕГО не отправляет в сеть.
    /// Используется PlayerNetworkController каждый тик.
    /// </summary>
    public sealed class MovementInputHandler : MonoBehaviour, IInputContextConsumer
    {
        private PlayerInputContext input;
        private bool bound;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction walkAction;
        private InputAction crouchAction;

        private PlayerInputState inputState;

        public PlayerInputState CurrentState => inputState;

        private float jumpBufferTimer;
        private const float JumpBufferTime = 0.15f;

        private void Update()
        {
            if (jumpBufferTimer > 0f)
            {
                jumpBufferTimer -= Time.deltaTime;
                inputState.Jump = true;
            }
            else
            {
                inputState.Jump = false;
            }
        }

        private void Awake()
        {
            if (input == null)
                input = GetComponent<PlayerInputContext>() ?? null;
        }

        // ======================================================
        // BIND
        // ======================================================

        public void BindInput(PlayerInputContext ctx)
        {
            input = ctx;
            if (input == null)
                return;

            var p = input.Actions.Player;

            moveAction   = p.FindAction("Move", true);
            jumpAction   = p.FindAction("Jump", true);
            sprintAction = p.FindAction("Sprint", true);
            walkAction   = p.FindAction("Walk", true);
            crouchAction = p.FindAction("Crouch", true);

            moveAction.Enable();
            jumpAction.Enable();
            sprintAction.Enable();
            walkAction.Enable();
            crouchAction.Enable();

            moveAction.performed += OnMove;
            moveAction.canceled  += OnMoveCanceled;

            jumpAction.performed += OnJump;

            sprintAction.performed += OnSprintStart;
            sprintAction.canceled  += OnSprintStop;

            walkAction.performed += OnWalkStart;
            walkAction.canceled  += OnWalkStop;

            crouchAction.performed += OnCrouch;
            crouchAction.canceled  += ctx => inputState.Crouch = false;

            bound = true;
        }

        // ======================================================
        // UNBIND
        // ======================================================

        public void UnbindInput(PlayerInputContext ctx)
        {
            if (!bound || input != ctx)
                return;

            moveAction.performed -= OnMove;
            moveAction.canceled  -= OnMoveCanceled;

            jumpAction.performed -= OnJump;

            sprintAction.performed -= OnSprintStart;
            sprintAction.canceled  -= OnSprintStop;

            walkAction.performed -= OnWalkStart;
            walkAction.canceled  -= OnWalkStop;

            crouchAction.performed -= OnCrouch;

            input = null;
            bound = false;
        }

        // ======================================================
        // INPUT HANDLERS
        // ======================================================

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
            jumpBufferTimer = JumpBufferTime;
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
            inputState.Crouch = true; // one-shot
        }

        // ======================================================
        // ONE-SHOT RESET
        // ======================================================

        // ======================================================
        // CAMERA INTEGRATION
        // ======================================================

        /// <summary>
        /// Камера записывает yaw сюда.
        /// </summary>
        public void SetYaw(float yaw)
        {
            inputState.Yaw = yaw;
        }

        public void SetPitch(float pitch)
        {
            inputState.Pitch = pitch;
        }

        // ======================================================
        // SAFETY
        // ======================================================

        private void OnDestroy()
        {
            if (input != null)
                UnbindInput(input);
        }


    }
}
