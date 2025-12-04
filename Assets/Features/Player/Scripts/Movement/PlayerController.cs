using UnityEngine;
using UnityEngine.InputSystem;
using Features.Abilities.Application;
using Features.Stats.UnityIntegration;

public class PlayerController : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCameraController playerCameraController;
    [SerializeField] private AbilityCaster abilityCaster;

    [Header("UI")]
    [SerializeField] private EnergyBarUI energyUI;
    [SerializeField] private HPBarUI hpUI;

    private InputSystem_Actions inputActions;

    // ============================================================
    // LIFECYCLE
    // ============================================================

    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        SetupMovement();
        SetupCamera();
        SetupAbilities();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        inputActions.Enable();

        // Подписка должна быть раньше Start других компонентов,
        // поэтому делаем её здесь, а не в Start().
        PlayerStats.OnStatsReady += HandleStatsReady;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        PlayerStats.OnStatsReady -= HandleStatsReady;
    }

    private void Start()
    {
        // Если PlayerRegistry успел инициализироваться до нашего Start —
        // просто обновляем UI вручную.
        var reg = PlayerRegistry.Instance;

        if (reg.LocalEnergy != null && reg.LocalHealth != null)
        {
            energyUI.Bind(reg.LocalEnergy);
            hpUI.Bind(reg.LocalHealth);
        }
    }

    // ============================================================
    // INPUT SETUP
    // ============================================================

    private void SetupMovement()
    {
        inputActions.Player.Move.performed += 
            ctx => playerMovement.SetMoveInput(ctx.ReadValue<Vector2>());
        inputActions.Player.Move.canceled += 
            _ => playerMovement.SetMoveInput(Vector2.zero);

        inputActions.Player.Jump.performed += _ => playerMovement.TryJump();

        inputActions.Player.Sprint.performed += _ => playerMovement.SetSprint(true);
        inputActions.Player.Sprint.canceled += _ => playerMovement.SetSprint(false);

        inputActions.Player.Walk.performed += _ => playerMovement.SetWalk(true);
        inputActions.Player.Walk.canceled += _ => playerMovement.SetWalk(false);

        inputActions.Player.Crouch.performed += _ => playerMovement.ToggleCrouch();
    }

    private void SetupCamera()
    {
        inputActions.Player.Look.performed += 
            ctx => playerCameraController.SetLookInput(ctx.ReadValue<Vector2>());
        inputActions.Player.Look.canceled += 
            _ => playerCameraController.SetLookInput(Vector2.zero);

        inputActions.Player.SwitchView.performed += _ => playerCameraController.SwitchView();
    }

    private void SetupAbilities()
    {
        abilityCaster ??= GetComponent<AbilityCaster>();

        inputActions.Player.Ability1.performed += _ => abilityCaster.TryCast(0);
        inputActions.Player.Ability2.performed += _ => abilityCaster.TryCast(1);
        inputActions.Player.Ability3.performed += _ => abilityCaster.TryCast(2);
        inputActions.Player.Ability4.performed += _ => abilityCaster.TryCast(3);
        inputActions.Player.Ability5.performed += _ => abilityCaster.TryCast(4);
    }

    // ============================================================
    // STATS READY CALLBACK
    // ============================================================

    private void HandleStatsReady(PlayerStats ps)
    {
        var reg = PlayerRegistry.Instance;

        if (reg.LocalEnergy == null || reg.LocalHealth == null)
        {
            Debug.LogError("[PlayerController] StatsReady fired but LocalEnergy/LocalHealth == null!");
            return;
        }

        energyUI.Bind(reg.LocalEnergy);
        hpUI.Bind(reg.LocalHealth);
    }
}
