using UnityEngine;
using UnityEngine.InputSystem;

// Этот скрипт вешается на Игрока рядом с EquipmentManager.
[RequireComponent(typeof(EquipmentManager))]
public class PlayerUsageController : MonoBehaviour
{
    private EquipmentManager equipmentManager;
    private InputSystem_Actions inputActions;

    private bool isUsingItem = false;
    private IUsable currentUsable;

    private void Awake()
    {
        equipmentManager = GetComponent<EquipmentManager>();

        inputActions = new InputSystem_Actions();
        inputActions.Player.Enable();

        // Логируем события создания
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
        // Вызываем Stop у того же предмета, который был нажат
        currentUsable?.OnUsePrimary_Stop();
        currentUsable = null; // Сбрасываем ссылку
    }

    private void Update()
    {
        // Если кнопка зажата, продолжаем вызывать Hold
        if (isUsingItem)
        {
            // Используем сохраненную ссылку, чтобы Hold и Stop
            // вызвались у одного и того же объекта
            currentUsable?.OnUsePrimary_Hold();
        }
    }
}