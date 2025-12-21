using UnityEngine;
using UnityEngine.InputSystem;
using Features.Abilities.Application;
using Features.Camera.UnityIntegration;
using Features.Player;

namespace Features.Player.UnityIntegration
{
    public class PlayerController : MonoBehaviour, IInputContextConsumer
    {
        [Header("Core")]
        [SerializeField] private PlayerMovementNetAdapter movementNet;
        [SerializeField] private PlayerCameraController playerCameraController;
        [SerializeField] private AbilityCasterNetAdapter abilityCasterNet;

        private PlayerInputContext input;
        private bool bound;

        // ======================================================
        // LIFECYCLE
        // ======================================================

        private void Awake()
        {
            if (movementNet == null)
                movementNet = GetComponent<PlayerMovementNetAdapter>();

            if (playerCameraController == null)
                playerCameraController = GetComponent<PlayerCameraController>();

            if (abilityCasterNet == null)
                abilityCasterNet = GetComponent<AbilityCasterNetAdapter>();

            if (movementNet == null)
                Debug.LogError($"[{name}] PlayerMovementNetAdapter not found", this);

            if (playerCameraController == null)
                Debug.LogError($"[{name}] PlayerCameraController not found", this);

            if (abilityCasterNet == null)
                Debug.LogError($"[{name}] AbilityCasterNetAdapter not found", this);
        }

        private bool Ready =>
            bound &&
            input != null &&
            movementNet != null &&
            playerCameraController != null &&
            abilityCasterNet != null;

        // ======================================================
        // INPUT BIND
        // ======================================================

        public void BindInput(PlayerInputContext ctx)
        {
            if (input == ctx)
                return;

            if (movementNet == null || playerCameraController == null || abilityCasterNet == null)
            {
                Debug.LogError($"[{name}] BindInput called but required refs are missing", this);
                return;
            }

            if (input != null)
                UnbindInput(input);

            input = ctx;
            if (input == null)
                return;

            BindMovement();
            BindCamera();
            BindAbilities();
            SetupCursor();

            bound = true;
        }

        public void UnbindInput(PlayerInputContext ctx)
        {
            if (!bound || input != ctx)
                return;

            UnbindMovement();
            UnbindCamera();
            UnbindAbilities();

            // Глушим всю карту, чтобы не прилетели колбэки в тот же кадр
            input.Actions.Player.Disable();

            input = null;
            bound = false;
        }

        // ======================================================
        // CURSOR
        // ======================================================

        private void SetupCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // ======================================================
        // MOVEMENT
        // ======================================================

        private void BindMovement()
        {
            var p = input.Actions.Player;

            Enable(p, "Move", "Jump", "Sprint", "Walk", "Crouch");

            p.FindAction("Move").performed += OnMove;
            p.FindAction("Move").canceled  += OnMoveCanceled;
            p.FindAction("Jump").performed += OnJump;

            p.FindAction("Sprint").performed += OnSprintStart;
            p.FindAction("Sprint").canceled  += OnSprintStop;

            p.FindAction("Walk").performed += OnWalkStart;
            p.FindAction("Walk").canceled  += OnWalkStop;

            p.FindAction("Crouch").performed += OnCrouch;
        }

        private void UnbindMovement()
        {
            if (input == null) return;

            var p = input.Actions.Player;

            p.FindAction("Move").performed -= OnMove;
            p.FindAction("Move").canceled  -= OnMoveCanceled;
            p.FindAction("Jump").performed -= OnJump;

            p.FindAction("Sprint").performed -= OnSprintStart;
            p.FindAction("Sprint").canceled  -= OnSprintStop;

            p.FindAction("Walk").performed -= OnWalkStart;
            p.FindAction("Walk").canceled  -= OnWalkStop;

            p.FindAction("Crouch").performed -= OnCrouch;

            Disable(p, "Move", "Jump", "Sprint", "Walk", "Crouch");
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            if (!Ready) return;
            movementNet.SendMoveInput(ctx.ReadValue<Vector2>());
        }

        private void OnMoveCanceled(InputAction.CallbackContext _)
        {
            if (!Ready) return;
            movementNet.SendMoveInput(Vector2.zero);
        }

        private void OnJump(InputAction.CallbackContext _)
        {
            if (!Ready) return;
            movementNet.Jump();
        }

        private void OnSprintStart(InputAction.CallbackContext _)
        {
            if (!Ready) return;
            movementNet.SetSprint(true);
        }

        private void OnSprintStop(InputAction.CallbackContext _)
        {
            if (!Ready) return;
            movementNet.SetSprint(false);
        }

        private void OnWalkStart(InputAction.CallbackContext _)
        {
            if (!Ready) return;
            movementNet.SetWalk(true);
        }

        private void OnWalkStop(InputAction.CallbackContext _)
        {
            if (!Ready) return;
            movementNet.SetWalk(false);
        }

        private void OnCrouch(InputAction.CallbackContext _)
        {
            if (!Ready) return;
            movementNet.ToggleCrouch();
        }

        // ======================================================
        // CAMERA
        // ======================================================

        private void BindCamera()
        {
            var p = input.Actions.Player;

            Enable(p, "Look", "SwitchView");

            p.FindAction("Look").performed += OnLook;
            p.FindAction("Look").canceled  += OnLookCanceled;
            p.FindAction("SwitchView").performed += OnSwitchView;
        }

        private void UnbindCamera()
        {
            if (input == null) return;

            var p = input.Actions.Player;

            p.FindAction("Look").performed -= OnLook;
            p.FindAction("Look").canceled  -= OnLookCanceled;
            p.FindAction("SwitchView").performed -= OnSwitchView;

            Disable(p, "Look", "SwitchView");
        }

        private void OnLook(InputAction.CallbackContext ctx)
        {
            if (!Ready) return;
            playerCameraController.SetLookInput(ctx.ReadValue<Vector2>());
        }

        private void OnLookCanceled(InputAction.CallbackContext _)
        {
            if (!Ready) return;
            playerCameraController.SetLookInput(Vector2.zero);
        }

        private void OnSwitchView(InputAction.CallbackContext _)
        {
            if (!Ready) return;
            playerCameraController.SwitchView();
        }

        // ======================================================
        // ABILITIES
        // ======================================================

        private void BindAbilities()
        {
            var p = input.Actions.Player;

            Enable(p, "Ability1", "Ability2", "Ability3", "Ability4", "Ability5");

            p.FindAction("Ability1").performed += _ => TryCast(0);
            p.FindAction("Ability2").performed += _ => TryCast(1);
            p.FindAction("Ability3").performed += _ => TryCast(2);
            p.FindAction("Ability4").performed += _ => TryCast(3);
            p.FindAction("Ability5").performed += _ => TryCast(4);
        }

        private void UnbindAbilities()
        {
            if (input == null) return;

            Disable(input.Actions.Player,
                "Ability1", "Ability2", "Ability3", "Ability4", "Ability5");
        }

        private void TryCast(int index)
        {
            if (!Ready) return;
            abilityCasterNet.Cast(index);
        }

        // ======================================================
        // HELPERS
        // ======================================================

        private static void Enable(InputActionMap map, params string[] names)
        {
            foreach (var n in names)
                map.FindAction(n, true).Enable();
        }

        private static void Disable(InputActionMap map, params string[] names)
        {
            foreach (var n in names)
                map.FindAction(n, true).Disable();
        }
    }
}
