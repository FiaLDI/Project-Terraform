using UnityEngine;
using UnityEngine.InputSystem;
using Features.Abilities.UnityIntegration;

public class PlayerController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCameraController playerCameraController;
    [SerializeField] private AbilityCaster abilityCaster;
    [SerializeField] private EnergyBarUI energyUI;
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private HPBarUI hpUI;
    [SerializeField] private PlayerHealth playerHealth;

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        // ========================================================
        // MOVEMENT
        // ========================================================
        inputActions.Player.Move.performed  += ctx => playerMovement.SetMoveInput(ctx.ReadValue<Vector2>());
        inputActions.Player.Move.canceled   += ctx => playerMovement.SetMoveInput(Vector2.zero);

        inputActions.Player.Jump.performed  += ctx => playerMovement.TryJump();

        inputActions.Player.Sprint.performed += ctx => playerMovement.SetSprint(true);
        inputActions.Player.Sprint.canceled  += ctx => playerMovement.SetSprint(false);

        inputActions.Player.Walk.performed   += ctx => playerMovement.SetWalk(true);
        inputActions.Player.Walk.canceled    += ctx => playerMovement.SetWalk(false);

        inputActions.Player.Crouch.performed += ctx => playerMovement.ToggleCrouch();

        // ========================================================
        // CAMERA
        // ========================================================
        inputActions.Player.Look.performed  += ctx => playerCameraController.SetLookInput(ctx.ReadValue<Vector2>());
        inputActions.Player.Look.canceled   += ctx => playerCameraController.SetLookInput(Vector2.zero);

        inputActions.Player.SwitchView.performed += ctx => playerCameraController.SwitchView();

        // ========================================================
        // ABILITIES → New AbilityService
        // ========================================================
        if (!abilityCaster)
             abilityCaster = GetComponent<AbilityCaster>();

        inputActions.Player.Ability1.performed += ctx => abilityCaster.TryCast(0);
        inputActions.Player.Ability2.performed += ctx => abilityCaster.TryCast(1);
        inputActions.Player.Ability3.performed += ctx => abilityCaster.TryCast(2);
        inputActions.Player.Ability4.performed += ctx => abilityCaster.TryCast(3);
        inputActions.Player.Ability5.performed += ctx => abilityCaster.TryCast(4);

        // ========================================================
        // CURSOR
        // ========================================================
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        // ========================================================
        // ENERGY UI
        // ========================================================
        if (energyUI != null && playerEnergy != null)
        {
            energyUI.Bind(playerEnergy);
            energyUI.UpdateImmediate(playerEnergy.CurrentEnergy, playerEnergy.MaxEnergy);
        }

        // ========================================================
        // HP UI
        // ========================================================
        if (hpUI != null && playerHealth != null)
        {
            hpUI.Bind(playerHealth);
            hpUI.UpdateImmediate(playerHealth.CurrentHp, playerHealth.MaxHp);
        }
    }

    // ========================================================
    // ENABLE / DISABLE INPUT
    // ========================================================
    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
}
