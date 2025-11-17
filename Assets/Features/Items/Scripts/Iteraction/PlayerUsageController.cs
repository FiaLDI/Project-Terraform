using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(EquipmentManager))]
public class PlayerUsageController : MonoBehaviour
{
    private EquipmentManager equipmentManager;
    private InputSystem_Actions inputActions;

    private bool isUsingItem = false;
    private IUsable currentUsable;

    [Header("Points")]
    public Transform throwPoint;       // NEW: Добавляем ThrowPoint
    public Camera playerCamera;        // NEW: Камера игрока

    private void Awake()
    {
        equipmentManager = GetComponent<EquipmentManager>();

        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();

        Debug.Log("[PlayerUsageController] Awake, subscribing input...");

        inputActions.Player.Use.performed += ctx => {
            Debug.Log("[PlayerUsageController] ACTION PERFORMED");
            OnUseStarted(ctx);
        };

        inputActions.Player.Use.canceled += ctx => {
            Debug.Log("[PlayerUsageController] ACTION CANCELED");
            OnUseCanceled(ctx);
        };
    }

    private void OnEnable() => inputActions?.Player.Enable();
    private void OnDisable() => inputActions?.Player.Disable();


    // NEW: Самый важный момент!!!
    // Вызывается EquipmentManager-ом при смене предмета
    public void OnItemEquipped(IUsable usable)
    {
        currentUsable = usable;

        // Если это бросаемый предмет — передаём ссылку на ThrowPoint
        if (usable is UsableThrowable throwable)
        {
            throwable.SetSpawnPoint(throwPoint);
            throwable.Initialize(playerCamera);
        }
    }


    private void OnUseStarted(InputAction.CallbackContext context)
    {
        Debug.Log("[PlayerUsageController] OnUseStarted");
        isUsingItem = true;

        currentUsable = equipmentManager.GetCurrentUsable();
        Debug.Log("[PlayerUsageController] currentUsable = " + currentUsable);

        currentUsable?.OnUsePrimary_Start();
    }


    private void OnUseCanceled(InputAction.CallbackContext context)
    {
        isUsingItem = false;

        currentUsable?.OnUsePrimary_Stop();
        currentUsable = null;
    }

    private void Update()
    {
        if (isUsingItem)
        {
            currentUsable?.OnUsePrimary_Hold();
        }
    }
}
