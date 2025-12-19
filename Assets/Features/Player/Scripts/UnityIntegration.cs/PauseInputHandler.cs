using UnityEngine;
using UnityEngine.InputSystem;
using Features.Player;

public class PauseInputHandler : MonoBehaviour
{
    [SerializeField] private PauseMenu pauseMenu;

    private PlayerInputContext input;

    private void OnEnable()
    {
        input = LocalPlayerContext.Get<PlayerInputContext>();

        if (input == null)
        {
            Debug.LogError("[PauseInputHandler] PlayerInputContext not found");
            enabled = false;
            return;
        }

        if (pauseMenu == null)
        {
            Debug.LogError("[PauseInputHandler] PauseMenu not assigned");
            enabled = false;
            return;
        }

        input.Actions.Player.Pause.performed += OnPause;
    }

    private void OnDisable()
    {
        if (input != null)
            input.Actions.Player.Pause.performed -= OnPause;
    }

    private void OnPause(InputAction.CallbackContext _)
    {
        // Если открыт любой UI — это Cancel
        if (UIStackManager.I.HasScreens)
        {
            UIStackManager.I.Pop();
            return;
        }

        // Иначе — открываем паузу
        pauseMenu.Open();
    }

}
