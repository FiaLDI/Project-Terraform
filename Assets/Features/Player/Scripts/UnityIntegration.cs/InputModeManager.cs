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
    public class InputModeManager : MonoBehaviour
{
    public static InputModeManager I;

    private PlayerInputContext input;
    private InputMode currentMode;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
    }

    private void Start()
    {
        input = LocalPlayerContext.Get<PlayerInputContext>();
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

        DisableAll();

        switch (mode)
        {
            case InputMode.Gameplay:
                EnableGameplay();
                break;

            case InputMode.Inventory:
            case InputMode.Dialog:
                EnableUI(false);
                break;

            case InputMode.Pause:
                EnableUI(true);
                break;

            case InputMode.Disabled:
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
    }

    private void EnableGameplay()
    {
        input.Actions.Player.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
    }

    private void EnableUI(bool pauseTime)
    {
        input.Actions.Player.Disable();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = pauseTime ? 0f : 1f;
    }
}

}
