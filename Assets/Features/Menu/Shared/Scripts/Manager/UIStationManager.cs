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

    private void OnEsc(InputAction.CallbackContext ctx)
    {
        if (SettingsMenuManager.I != null && SettingsMenuManager.I.SettingsMenuOpen)
        {
            SettingsMenuManager.I.CloseSettings();
            return;
        }

        if (ActiveStation != null)
        {
            ActiveStation.Toggle();
            ActiveStation = null;
            return;
        }

        if (InventoryManager.instance != null && InventoryManager.instance.IsOpen)
        {
            InventoryManager.instance.SetOpen(false);
            return;
        }

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
