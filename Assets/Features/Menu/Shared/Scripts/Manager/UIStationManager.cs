using UnityEngine;
using UnityEngine.InputSystem;

public class UIStationManager : MonoBehaviour
{
    public static UIStationManager I;

    public BaseStationUI ActiveStation { get; private set; }

    [Header("Inputs")]
    [SerializeField] private InputActionReference escAction;

    private void Awake()
    {
        if (I == null) I = this;
        else Destroy(gameObject);

        if (escAction != null)
        {
            escAction.action.performed += OnEsc;
            escAction.action.Enable();
        }
    }

    private void OnDestroy()
    {
        if (escAction != null)
        {
            escAction.action.performed -= OnEsc;
            escAction.action.Disable();
        }
    }

    // -----------------------------------------------------------
    //   ЕДИНСТВЕННО ПРАВИЛЬНЫЙ ПОРЯДОК ОБРАБОТКИ ESC
    // -----------------------------------------------------------
    private void OnEsc(InputAction.CallbackContext ctx)
    {
        // 1️⃣ Закрыть настройки
        if (SettingsMenuManager.I != null && SettingsMenuManager.I.SettingsMenuOpen)
        {
            SettingsMenuManager.I.CloseSettings();
            return;
        }

        // 2️⃣ Закрыть станцию
        if (ActiveStation != null)
        {
            ActiveStation.Toggle();
            ActiveStation = null;
            return;
        }

        // 3️⃣ Pause Menu (открыть / закрыть)
        PauseMenu.I.Toggle();
    }

    // -----------------------------------------------------------
    public void OpenStation(BaseStationUI station)
    {
        ActiveStation = station;
    }

    public void CloseStation(BaseStationUI station)
    {
        if (ActiveStation == station)
            ActiveStation = null;
    }
}
