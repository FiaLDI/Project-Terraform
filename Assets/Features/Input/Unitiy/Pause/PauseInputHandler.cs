using UnityEngine;
using UnityEngine.InputSystem;
using Features.Input;
using Features.Player;

public class PauseInputHandler : MonoBehaviour, IInputContextConsumer
{
    private PlayerInputContext input;
    private InputAction pauseAction;
    private PauseMenu pauseMenu;
    private bool subscribed;

    public void BindInput(PlayerInputContext ctx)
    {
        if (input == ctx)
            return;

        if (input != null)
                UnbindInput(input);
        input = ctx;

        if (input == null)
            return;

        if (pauseMenu == null)
        {
            pauseMenu = Object.FindAnyObjectByType<PauseMenu>(
                FindObjectsInactive.Include);

            if (pauseMenu == null)
            {
                Debug.LogError("[PauseInputHandler] PauseMenu not found");
                return;
            }
        }

        pauseAction = input.Actions.Player.FindAction("Pause", true);
        pauseAction.performed += OnPause;
        pauseAction.Enable();

        subscribed = true;
    }

    public void UnbindInput(PlayerInputContext ctx)
    {
        if (!subscribed || input != ctx)
            return;

        if (pauseAction != null)
        {
            pauseAction.performed -= OnPause;
            pauseAction.Disable();
            pauseAction = null;
        }

        input = null;
        subscribed = false;
    }

    private void OnPause(InputAction.CallbackContext _)
    {
        if (UIStackManager.I != null &&
            UIStackManager.I.IsTop<PauseMenu>())
        {
            UIStackManager.I.Pop();
        }
        else
        {
            pauseMenu.Open();
        }
    }
}
