using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(EquipmentManager))]
public class PlayerUsageController : MonoBehaviour
{
    public static bool InteractionLocked = false; // UI блокировка Use

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

        // Подписка на действия Use (ЛКМ или другая кнопка)
        inputActions.Player.Use.performed += ctx => OnUseStarted();
        inputActions.Player.Use.canceled += ctx => OnUseCanceled();
    }

    private void OnEnable()  => inputActions?.Player.Enable();
    private void OnDisable() => inputActions?.Player.Disable();


    // Получение ссылки на IUsable предмет при экипировке
    public void OnItemEquipped(IUsable usable)
    {
        currentUsable = usable;

        if (usable is UsableThrowable throwable)
        {
            throwable.SetSpawnPoint(throwPoint);
            throwable.Initialize(playerCamera);
        }
    }

    // ======================
    //      USE START
    // ======================
    private void OnUseStarted()
    {
        if (InteractionLocked) return; // UI открыт

        isUsingItem = true;

        currentUsable = equipmentManager.GetCurrentUsable();
        currentUsable?.OnUsePrimary_Start();
    }

    // ======================
    //       USE STOP
    // ======================
    private void OnUseCanceled()
    {
        if (InteractionLocked) return;

        isUsingItem = false;

        currentUsable?.OnUsePrimary_Stop();
        currentUsable = null;
    }

    // ======================
    //        UPDATE
    // ======================
    private void Update()
    {
        if (!InteractionLocked && isUsingItem)
        {
            currentUsable?.OnUsePrimary_Hold();
        }
    }
}
