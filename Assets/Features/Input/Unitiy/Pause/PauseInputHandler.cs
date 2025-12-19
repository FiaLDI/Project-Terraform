using UnityEngine;
using UnityEngine.InputSystem;
using Features.Input;
using Features.Player;

public class PauseInputHandler : MonoBehaviour, IInputContextConsumer
{
    private InputAction pauseAction;
    private PauseMenu pauseMenu;

    public void BindInput(PlayerInputContext ctx)
    {
        pauseMenu = FindObjectOfType<PauseMenu>(true);
        if (pauseMenu == null)
        {
            Debug.LogError("[PauseInputHandler] PauseMenu not found");
            return;
        }

        pauseAction = ctx.Actions.Player.FindAction("Pause", true);
        pauseAction.Enable();
        pauseAction.performed += OnPause;

        Debug.Log("[PauseInputHandler] Pause bound");
    }

    private void OnDisable()
    {
        if (pauseAction != null)
            pauseAction.performed -= OnPause;
    }

    private void OnPause(InputAction.CallbackContext _)
    {
        if (UIStackManager.I != null && UIStackManager.I.IsTop<PauseMenu>())
            UIStackManager.I.Pop();
        else
            pauseMenu.Open();
    }
}
