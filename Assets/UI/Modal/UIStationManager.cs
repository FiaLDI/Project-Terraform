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

    public void OpenStation(BaseStationUI station)
    {
        ActiveStation = station;
        station.Open();
    }

    public void CloseStation(BaseStationUI station)
    {
        if (ActiveStation == station)
            ActiveStation = null;
    }
}
