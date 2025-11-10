using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCameraController playerCameraController;


    void OnDestroy()
    {
        Debug.LogError($"💀 {name} уничтожен (OnDestroy) {gameObject.scene.name}", this);
        Debug.LogError(StackTraceUtility.ExtractStackTrace());
    }

    //[Header("Inventory References")]
    //[SerializeField] private InventoryManager inventoryManager;
    //[SerializeField] private EquipmentManager equipmentManager;
    //[Header("Interaction References")]
    //[SerializeField] private PlayerInteraction playerInteraction;

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();

        // === MOVEMENT ===
        inputActions.Player.Move.performed += ctx => playerMovement.SetMoveInput(ctx.ReadValue<Vector2>());
        inputActions.Player.Move.canceled += ctx => playerMovement.SetMoveInput(Vector2.zero);

        inputActions.Player.Jump.performed += ctx => playerMovement.TryJump();

        inputActions.Player.Sprint.performed += ctx => playerMovement.SetSprint(true);
        inputActions.Player.Sprint.canceled += ctx => playerMovement.SetSprint(false);

        inputActions.Player.Walk.performed += ctx => playerMovement.SetWalk(true);
        inputActions.Player.Walk.canceled += ctx => playerMovement.SetWalk(false);

        inputActions.Player.Crouch.performed += ctx => playerMovement.ToggleCrouch();

        // === CAMERA ===
        inputActions.Player.Look.performed += ctx => playerCameraController.SetLookInput(ctx.ReadValue<Vector2>());
        inputActions.Player.Look.canceled += ctx => playerCameraController.SetLookInput(Vector2.zero);
        inputActions.Player.SwitchView.performed += ctx => playerCameraController.SwitchView();

        // === INVENTORY & ITEMS ===
        //inputActions.UI.OpenInventory.performed += ctx => ToggleInventory();
        //inputActions.UI.EquipFirst.performed += ctx => inventoryManager.SelectHotbarSlot(0);
        //inputActions.UI.EquipSecond.performed += ctx => inventoryManager.SelectHotbarSlot(1);

        //inputActions.Player.Interact.performed += ctx => playerInteraction.TryInteract();
        //inputActions.Player.Drop.performed += ctx => playerInteraction.DropCurrentItem(false);
        //inputActions.Player.Use.performed += ctx => equipmentManager.TryUseCurrentItem();
        //inputActions.Player.AltUse.performed += ctx => equipmentManager.TryAltUseCurrentItem();
        //inputActions.Player.Drop.performed += ctx => inventoryManager.DropItemFromSelectedSlot();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();
    //private void ToggleInventory()
    //{
    //    if (inventoryManager == null) return;

    //    bool newState = !inventoryManager.IsOpen;
    //    inventoryManager.SetOpen(newState);

    //    if (newState)
    //    {
    //        Cursor.lockState = CursorLockMode.None;
    //        Cursor.visible = true;
    //        playerCameraController.enabled = false;
    //    }
    //    else
    //    {
    //        Cursor.lockState = CursorLockMode.Locked;
    //        Cursor.visible = false;
    //        playerCameraController.enabled = true;
    //    }
    //}

}
