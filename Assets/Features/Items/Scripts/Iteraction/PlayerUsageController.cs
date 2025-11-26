using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(EquipmentManager))]
public class PlayerUsageController : MonoBehaviour
{
    public static bool InteractionLocked = false;

    private EquipmentManager equipmentManager;
    private InputSystem_Actions inputActions;

    private bool isUsingItem = false;
    private IUsable currentUsable;

    [Header("Points")]
    public Transform throwPoint;
    public Camera playerCamera;


    private void Awake()
    {
        equipmentManager = GetComponent<EquipmentManager>();

        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();

        inputActions.Player.Use.performed += ctx => OnUseStarted();
        inputActions.Player.Use.canceled += ctx => OnUseCanceled();

        inputActions.Player.SecondaryUse.performed += ctx => {
            currentUsable?.OnUseSecondary_Start();
        };

        inputActions.Player.SecondaryUse.canceled += ctx => {
            currentUsable?.OnUseSecondary_Stop();
        };
    }

    private void OnEnable()  => inputActions?.Player.Enable();
    private void OnDisable() => inputActions?.Player.Disable();


    public void OnItemEquipped(IUsable usable)
    {
        currentUsable = usable;

        if (usable is UsableThrowable throwable)
        {
            throwable.SetSpawnPoint(throwPoint);
            throwable.Initialize(playerCamera);
        }
    }

    private void OnUseStarted()
    {
        if (InteractionLocked) return; 

        isUsingItem = true;

        currentUsable = equipmentManager.GetCurrentUsable();
        currentUsable?.OnUsePrimary_Start();
    }

    private void OnUseCanceled()
    {
        if (InteractionLocked) return;

        isUsingItem = false;

        currentUsable?.OnUsePrimary_Stop();
        currentUsable = null;
    }

    private void Update()
    {
        if (!InteractionLocked && isUsingItem)
        {
            currentUsable?.OnUsePrimary_Hold();
        }

        if (inputActions.Player.SecondaryUse.IsPressed())
        {
            currentUsable?.OnUseSecondary_Hold();
        }
    }
}
