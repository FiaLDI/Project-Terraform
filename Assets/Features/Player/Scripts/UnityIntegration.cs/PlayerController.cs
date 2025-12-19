using UnityEngine;
using UnityEngine.InputSystem;
using Features.Abilities.Application;
using Features.Camera.UnityIntegration;
using Features.Input;
using Features.Player;

namespace Features.Player.UnityIntegration
{
    public class PlayerController : MonoBehaviour, IInputContextConsumer
    {
        [Header("Core")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerCameraController playerCameraController;
        [SerializeField] private AbilityCaster abilityCaster;

        private PlayerInputContext input;
        private bool subscribed;

        // ======================================================
        // INPUT BIND
        // ======================================================

        public void BindInput(PlayerInputContext ctx)
        {
            if (input == ctx)
                return;

            Unsubscribe();
            input = ctx;

            if (input == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerController)}: BindInput with NULL",
                    this);
                return;
            }

            Subscribe();
        }

        private void OnEnable()
        {
            if (input != null)
                Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        // ======================================================
        // SUBSCRIBE / UNSUBSCRIBE
        // ======================================================

        private void Subscribe()
        {
            if (subscribed || input == null)
                return;

            if (abilityCaster == null)
                abilityCaster = GetComponent<AbilityCaster>();

            BindMovement();
            BindCamera();
            BindAbilities();
            SetupCursor();

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || input == null)
                return;

            UnbindMovement();
            UnbindCamera();
            UnbindAbilities();

            subscribed = false;
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
            Debug.Log("[DBG] Player Move enabled = " +
    input.Actions.Player.FindAction("Move").enabled);
            var p = input.Actions.Player;

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
            var p = input.Actions.Player;

            p.FindAction("Move").performed -= OnMove;
            p.FindAction("Move").canceled  -= OnMoveCanceled;

            p.FindAction("Jump").performed -= OnJump;

            p.FindAction("Sprint").performed -= OnSprintStart;
            p.FindAction("Sprint").canceled  -= OnSprintStop;

            p.FindAction("Walk").performed -= OnWalkStart;
            p.FindAction("Walk").canceled  -= OnWalkStop;

            p.FindAction("Crouch").performed -= OnCrouch;
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            playerMovement.SetMoveInput(ctx.ReadValue<Vector2>());
        }

        private void OnMoveCanceled(InputAction.CallbackContext _)
        {
            playerMovement.SetMoveInput(Vector2.zero);
        }

        private void OnJump(InputAction.CallbackContext _)
        {
            playerMovement.TryJump();
        }

        private void OnSprintStart(InputAction.CallbackContext _)
        {
            playerMovement.SetSprint(true);
        }

        private void OnSprintStop(InputAction.CallbackContext _)
        {
            playerMovement.SetSprint(false);
        }

        private void OnWalkStart(InputAction.CallbackContext _)
        {
            playerMovement.SetWalk(true);
        }

        private void OnWalkStop(InputAction.CallbackContext _)
        {
            playerMovement.SetWalk(false);
        }

        private void OnCrouch(InputAction.CallbackContext _)
        {
            playerMovement.ToggleCrouch();
        }

        // ======================================================
        // CAMERA
        // ======================================================

        private void BindCamera()
        {
            var p = input.Actions.Player;

            p.FindAction("Look").performed += OnLook;
            p.FindAction("Look").canceled  += OnLookCanceled;

            p.FindAction("SwitchView").performed += OnSwitchView;
        }

        private void UnbindCamera()
        {
            var p = input.Actions.Player;

            p.FindAction("Look").performed -= OnLook;
            p.FindAction("Look").canceled  -= OnLookCanceled;

            p.FindAction("SwitchView").performed -= OnSwitchView;
        }

        private void OnLook(InputAction.CallbackContext ctx)
        {
            playerCameraController.SetLookInput(ctx.ReadValue<Vector2>());
        }

        private void OnLookCanceled(InputAction.CallbackContext _)
        {
            playerCameraController.SetLookInput(Vector2.zero);
        }

        private void OnSwitchView(InputAction.CallbackContext _)
        {
            playerCameraController.SwitchView();
        }

        // ======================================================
        // ABILITIES
        // ======================================================

        private void BindAbilities()
        {
            var p = input.Actions.Player;

            p.FindAction("Ability1").performed += OnAbility1;
            p.FindAction("Ability2").performed += OnAbility2;
            p.FindAction("Ability3").performed += OnAbility3;
            p.FindAction("Ability4").performed += OnAbility4;
            p.FindAction("Ability5").performed += OnAbility5;
        }

        private void UnbindAbilities()
        {
            var p = input.Actions.Player;

            p.FindAction("Ability1").performed -= OnAbility1;
            p.FindAction("Ability2").performed -= OnAbility2;
            p.FindAction("Ability3").performed -= OnAbility3;
            p.FindAction("Ability4").performed -= OnAbility4;
            p.FindAction("Ability5").performed -= OnAbility5;
        }

        private void OnAbility1(InputAction.CallbackContext _) => abilityCaster.TryCast(0);
        private void OnAbility2(InputAction.CallbackContext _) => abilityCaster.TryCast(1);
        private void OnAbility3(InputAction.CallbackContext _) => abilityCaster.TryCast(2);
        private void OnAbility4(InputAction.CallbackContext _) => abilityCaster.TryCast(3);
        private void OnAbility5(InputAction.CallbackContext _) => abilityCaster.TryCast(4);
    }
}
