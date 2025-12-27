using UnityEngine;
using UnityEngine.InputSystem;
using Features.Input;
using Features.Player;
using Features.Camera.UnityIntegration;
using Features.Player.UnityIntegration;

namespace Features.Camera.UnityIntegration
{
    public sealed class CameraInputHandler : MonoBehaviour, IInputContextConsumer
    {
        private PlayerInputContext input;
        private bool bound;

        private InputAction lookAction;
        private InputAction switchViewAction;

        public void BindInput(PlayerInputContext ctx)
        {
            if (input == ctx) return;
            if (input != null) UnbindInput(input);

            input = ctx;
            if (input == null) return;

            var p = input.Actions.Player;
            Enable(p, "Look", "SwitchView");

            lookAction = p.FindAction("Look", true);
            switchViewAction = p.FindAction("SwitchView", true);

            lookAction.performed += OnLook;
            lookAction.canceled += OnLookCanceled;
            switchViewAction.performed += OnSwitchView;

            SetupCursor();
            bound = true;
        }

        public void UnbindInput(PlayerInputContext ctx)
        {
            if (!bound || input != ctx) return;

            if (lookAction != null) { lookAction.performed -= OnLook; lookAction.canceled -= OnLookCanceled; }
            if (switchViewAction != null) switchViewAction.performed -= OnSwitchView;

            input = null;
            bound = false;
        }

        private void SetupCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // --- HANDLERS ---

        private PlayerCameraController GetCameraCtrl()
        {
            var player = LocalPlayerContext.Player;
            return player != null ? player.GetComponent<PlayerCameraController>() : null;
        }

        private void OnLook(InputAction.CallbackContext ctx)
        {
            GetCameraCtrl()?.SetLookInput(ctx.ReadValue<Vector2>());
        }

        private void OnLookCanceled(InputAction.CallbackContext ctx)
        {
            GetCameraCtrl()?.SetLookInput(Vector2.zero);
        }

        private void OnSwitchView(InputAction.CallbackContext ctx)
        {
            GetCameraCtrl()?.SwitchView();
        }

        private static void Enable(InputActionMap map, params string[] names)
        {
            foreach (var n in names) map.FindAction(n, true).Enable();
        }
    }
}
