using UnityEngine;
using UnityEngine.InputSystem;
using Features.Input;
using Features.Player;

public class UIBackInputHandler : MonoBehaviour, IInputContextConsumer
{
    private InputAction cancelAction;

    public void BindInput(PlayerInputContext ctx)
    {
        cancelAction = ctx.Actions.UI.FindAction("Cancel", true);
        cancelAction.Enable();
        cancelAction.performed += OnCancel;
    }

    private void OnDisable()
    {
        if (cancelAction != null)
            cancelAction.performed -= OnCancel;
    }

    private void OnCancel(InputAction.CallbackContext _)
    {
        if (UIStackManager.I != null && UIStackManager.I.HasScreens)
            UIStackManager.I.Pop();
    }
}
