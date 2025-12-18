using UnityEngine;
using UnityEngine.InputSystem;

public class UIStationManager : MonoBehaviour
{
    public static UIStationManager Instance { get; private set; }

    public BaseStationUI ActiveStation { get; private set; }

    [Header("Inputs")]
    [SerializeField] private InputActionReference escAction;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

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

    private void OnEsc(InputAction.CallbackContext ctx)
    {
        // 1. Settings
        if (SettingsMenuManager.I != null &&
            SettingsMenuManager.I.SettingsMenuOpen)
        {
            SettingsMenuManager.I.CloseSettings();
            return;
        }

        // 2. Station
        if (ActiveStation != null)
        {
            ActiveStation.Toggle();
            ActiveStation = null;
            return;
        }

        // 3. Pause
        PauseMenu.I?.Toggle();
    }

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
