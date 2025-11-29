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

    public bool IsAnyStationOpen => ActiveStation != null && ActiveStation.IsOpen;

    private void OnEsc(InputAction.CallbackContext ctx)
    {
        if (ActiveStation != null)
        {
            ActiveStation.Toggle();
            ActiveStation = null;
            return;
        }

        PauseMenu.I.Toggle();
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
