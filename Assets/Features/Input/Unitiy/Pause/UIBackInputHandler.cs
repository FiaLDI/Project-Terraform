using UnityEngine;
using UnityEngine.InputSystem;
using Features.Input;
using Features.Player;

public class UIBackInputHandler : MonoBehaviour, IInputContextConsumer
{
    private PlayerInputContext input;
    private InputAction cancelAction;
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

        cancelAction = input.Actions.UI.FindAction("Cancel", true);
        cancelAction.performed += OnCancel;
        cancelAction.Enable();

        subscribed = true;
    }

    public void UnbindInput(PlayerInputContext ctx)
    {
        if (!subscribed || input != ctx)
            return;

        if (cancelAction != null)
        {
            cancelAction.performed -= OnCancel;
            cancelAction.Disable();
            cancelAction = null;
        }

        input = null;
        subscribed = false;
    }

    private void OnCancel(InputAction.CallbackContext _)
    {
        if (UIStackManager.I != null && UIStackManager.I.HasScreens)
            UIStackManager.I.Pop();
    }
}
