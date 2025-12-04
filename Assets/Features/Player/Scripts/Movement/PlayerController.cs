using UnityEngine;
using UnityEngine.InputSystem;
using Features.Abilities.Application;
using Features.Stats.Adapter;          // ← для EnergyStatsAdapter / MovementStatsAdapter / HealthStatsAdapter
using Features.Energy.Domain;         // ← IEnergy

public class PlayerController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCameraController playerCameraController;
    [SerializeField] private AbilityCaster abilityCaster;

    [Header("UI")]
    [SerializeField] private EnergyBarUI energyUI;
    [SerializeField] private HPBarUI hpUI;

    [Header("Stats Adapters")]
    [SerializeField] private EnergyStatsAdapter energyAdapter;
    [SerializeField] private HealthStatsAdapter healthAdapter;

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        // ========================================================
        // MOVEMENT INPUT
        // ========================================================
        inputActions.Player.Move.performed += ctx => playerMovement.SetMoveInput(ctx.ReadValue<Vector2>());
        inputActions.Player.Move.canceled += ctx => playerMovement.SetMoveInput(Vector2.zero);

        inputActions.Player.Jump.performed += ctx => playerMovement.TryJump();

        inputActions.Player.Sprint.performed += ctx => playerMovement.SetSprint(true);
        inputActions.Player.Sprint.canceled += ctx => playerMovement.SetSprint(false);

        inputActions.Player.Walk.performed += ctx => playerMovement.SetWalk(true);
        inputActions.Player.Walk.canceled += ctx => playerMovement.SetWalk(false);

        inputActions.Player.Crouch.performed += ctx => playerMovement.ToggleCrouch();

        // ========================================================
        // CAMERA INPUT
        // ========================================================
        inputActions.Player.Look.performed += ctx => playerCameraController.SetLookInput(ctx.ReadValue<Vector2>());
        inputActions.Player.Look.canceled += ctx => playerCameraController.SetLookInput(Vector2.zero);

        inputActions.Player.SwitchView.performed += ctx => playerCameraController.SwitchView();

        // ========================================================
        // ABILITIES
        // ========================================================
        if (!abilityCaster)
            abilityCaster = GetComponent<AbilityCaster>();

        inputActions.Player.Ability1.performed += ctx => abilityCaster.TryCast(0);
        inputActions.Player.Ability2.performed += ctx => abilityCaster.TryCast(1);
        inputActions.Player.Ability3.performed += ctx => abilityCaster.TryCast(2);
        inputActions.Player.Ability4.performed += ctx => abilityCaster.TryCast(3);
        inputActions.Player.Ability5.performed += ctx => abilityCaster.TryCast(4);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        // ========================================================
        // ENERGY UI (через IEnergy → EnergyStatsAdapter)
        // ========================================================
        if (energyUI != null && energyAdapter != null)
        {
            IEnergy energy = energyAdapter;          // адаптер реализует IEnergy
            energyUI.Bind(energyAdapter);
        }

        // ========================================================
        // HP UI
        // ========================================================
        if (hpUI != null && healthAdapter != null)
        {
            hpUI.Bind(healthAdapter);
        }
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
