using UnityEngine;
using Features.Player;

public enum InputMode
{
    Gameplay,
    Inventory,
    Pause,
    Dialog,
    Disabled
}

namespace Features.Input
{
    [DefaultExecutionOrder(-100)]
    public sealed class InputModeManager : MonoBehaviour
    {
        public static InputModeManager I { get; private set; }

        private PlayerInputContext input;
        private InputMode currentMode = InputMode.Disabled;

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            I = this;
            DontDestroyOnLoad(gameObject);
        }

        // ======================================================
        // BIND
        // ======================================================

        public void Bind(PlayerInputContext inputContext)
        {
            input = inputContext;

            if (input == null)
            {
                Debug.LogError("[InputModeManager] Bind called with NULL input");
                return;
            }

            input.PlayerInput.SwitchCurrentActionMap("Player");

            SetMode(InputMode.Gameplay);
        }


        // ======================================================
        // PUBLIC API
        // ======================================================

        public void SetMode(InputMode mode)
        {
            if (currentMode == mode)
                return;

            currentMode = mode;

            if (input == null)
            {
                Debug.LogWarning(
                    $"[InputModeManager] SetMode({mode}) called before Bind"
                );
                return;
            }

            switch (mode)
            {
                case InputMode.Gameplay:
                    EnableGameplay();
                    break;

                case InputMode.Inventory:
                case InputMode.Dialog:
                    EnableUI(pauseTime: false);
                    break;

                case InputMode.Pause:
                    EnableUI(pauseTime: true);
                    break;

                case InputMode.Disabled:
                    DisableAll();
                    break;
            }
        }

        public InputMode CurrentMode => currentMode;

        // ======================================================
        // INTERNAL
        // ======================================================

        private void DisableAll()
        {
            input.Actions.Player.Disable();
            input.Actions.UI.Disable();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void EnableGameplay()
        {
            input.Actions.UI.Disable();
            input.Actions.Player.Enable();

            input.PlayerInput.SwitchCurrentActionMap("Player");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            Debug.Log(
    $"[InputModeManager] Gameplay enabled | " +
    $"PlayerEnabled={input.Actions.Player.enabled} | " +
    $"Map={input.PlayerInput.currentActionMap?.name}"
);
        }

        private void EnableUI(bool pauseTime)
        {
            input.Actions.Player.Disable();
            input.Actions.UI.Enable();

            input.PlayerInput.SwitchCurrentActionMap("UI");

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = pauseTime ? 0f : 1f;
        }
    }
}
