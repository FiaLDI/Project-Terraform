using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCameraController playerCameraController;

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        inputActions.Enable();

        inputActions.Player.Move.performed += ctx => playerMovement.SetMoveInput(ctx.ReadValue<Vector2>());
        inputActions.Player.Move.canceled += ctx => playerMovement.SetMoveInput(Vector2.zero);

        inputActions.Player.Jump.performed += ctx => playerMovement.TryJump();

        inputActions.Player.Sprint.performed += ctx => playerMovement.SetSprint(true);
        inputActions.Player.Sprint.canceled += ctx => playerMovement.SetSprint(false);

        inputActions.Player.Walk.performed += ctx => playerMovement.SetWalk(true);
        inputActions.Player.Walk.canceled += ctx => playerMovement.SetWalk(false);

        inputActions.Player.Crouch.performed += ctx => playerMovement.ToggleCrouch();

        inputActions.Player.Look.performed += ctx => playerCameraController.SetLookInput(ctx.ReadValue<Vector2>());
        inputActions.Player.Look.canceled += ctx => playerCameraController.SetLookInput(Vector2.zero);

        inputActions.Player.SwitchView.performed += ctx => playerCameraController.SwitchView();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
}
