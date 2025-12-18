using UnityEngine;
using UnityEngine.InputSystem;
using Features.Abilities.Application;
using Features.Camera.UnityIntegration;

namespace Features.Player.UnityIntegration
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerCameraController playerCameraController;
        [SerializeField] private AbilityCaster abilityCaster;

        private PlayerInputContext input;
        private bool subscribed;

        // ======================================================
        // LIFECYCLE
        // ======================================================

        private void OnEnable()
        {
            // Безопасно получаем PlayerInputContext
            if (input == null)
                input = GetComponent<PlayerInputContext>();

            Debug.Log(
                    $"[TEST] input={(input != null)} " +
                    $"actions={(input?.Actions != null)} " +
                    $"playerMap={(input?.Actions?.Player != null)} " +
                    $"uiMap={(input?.Actions?.UI != null)}",
                    this
                );

            if (input == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerController)}: {nameof(PlayerInputContext)} not found",
                    this);
                return;
            }

            if (abilityCaster == null)
                abilityCaster = GetComponent<AbilityCaster>();

            BindMovement();
            BindCamera();
            BindAbilities();
            SetupCursor();

            subscribed = true;
        }

        private void OnDisable()
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
            var p = input.Actions.Player;

            p.Move.performed += OnMove;
            p.Move.canceled  += OnMoveCanceled;

            p.Jump.performed   += OnJump;
            p.Sprint.performed += OnSprintStart;
            p.Sprint.canceled  += OnSprintStop;

            p.Walk.performed += OnWalkStart;
            p.Walk.canceled  += OnWalkStop;

            p.Crouch.performed += OnCrouch;
        }

        private void UnbindMovement()
        {
            var p = input.Actions.Player;

            p.Move.performed -= OnMove;
            p.Move.canceled  -= OnMoveCanceled;

            p.Jump.performed   -= OnJump;
            p.Sprint.performed -= OnSprintStart;
            p.Sprint.canceled  -= OnSprintStop;

            p.Walk.performed -= OnWalkStart;
            p.Walk.canceled  -= OnWalkStop;

            p.Crouch.performed -= OnCrouch;
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

            p.Look.performed += OnLook;
            p.Look.canceled  += OnLookCanceled;
            p.SwitchView.performed += OnSwitchView;
        }

        private void UnbindCamera()
        {
            var p = input.Actions.Player;

            p.Look.performed -= OnLook;
            p.Look.canceled  -= OnLookCanceled;
            p.SwitchView.performed -= OnSwitchView;
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

            p.Ability1.performed += OnAbility1;
            p.Ability2.performed += OnAbility2;
            p.Ability3.performed += OnAbility3;
            p.Ability4.performed += OnAbility4;
            p.Ability5.performed += OnAbility5;
        }

        private void UnbindAbilities()
        {
            var p = input.Actions.Player;

            p.Ability1.performed -= OnAbility1;
            p.Ability2.performed -= OnAbility2;
            p.Ability3.performed -= OnAbility3;
            p.Ability4.performed -= OnAbility4;
            p.Ability5.performed -= OnAbility5;
        }

        private void OnAbility1(InputAction.CallbackContext _) => abilityCaster.TryCast(0);
        private void OnAbility2(InputAction.CallbackContext _) => abilityCaster.TryCast(1);
        private void OnAbility3(InputAction.CallbackContext _) => abilityCaster.TryCast(2);
        private void OnAbility4(InputAction.CallbackContext _) => abilityCaster.TryCast(3);
        private void OnAbility5(InputAction.CallbackContext _) => abilityCaster.TryCast(4);
    }
}
