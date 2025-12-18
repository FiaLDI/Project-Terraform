using UnityEngine;

public class UIStationManager : MonoBehaviour
{
    public static UIStationManager Instance { get; private set; }

    public BaseStationUI ActiveStation { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // ======================================================
    // CANCEL HANDLING (вызывается извне)
    // ======================================================

    public void HandleCancel()
    {
        // 1. Settings
        if (SettingsMenuManager.I != null &&
            SettingsMenuManager.I.SettingsMenuOpen)
        {
            SettingsMenuManager.I.CloseSettings();
            return;
        }

        // 2. Station UI
        if (ActiveStation != null)
        {
            ActiveStation.Toggle();
            ActiveStation = null;
            return;
        }

        // 3. Pause
        PauseMenu.I?.Toggle();
    }

    // ======================================================
    // STATIONS
    // ======================================================

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
