using UnityEngine;

public sealed class CampaignRunContext : MonoBehaviour
{
    public static CampaignRunContext I { get; private set; }

    public string PlanetId { get; private set; } = string.Empty;
    public string BiomeId { get; private set; } = string.Empty;
    public string WorldConfigId { get; private set; } = string.Empty;
    public int ThreatLevel { get; private set; } = 1;
    public int ShipThreatCap { get; private set; } = 1;

    public bool HasActiveRun =>
        !string.IsNullOrWhiteSpace(PlanetId) &&
        !string.IsNullOrWhiteSpace(BiomeId) &&
        !string.IsNullOrWhiteSpace(WorldConfigId);

    public static CampaignRunContext EnsureExists()
    {
        if (I != null)
            return I;

        var existing = FindFirstObjectByType<CampaignRunContext>();
        if (existing != null)
            return existing;

        var go = new GameObject(nameof(CampaignRunContext));
        return go.AddComponent<CampaignRunContext>();
    }

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Set(string planetId, string biomeId, string worldConfigId, int threatLevel, int shipThreatCap = 1)
    {
        PlanetId = planetId ?? string.Empty;
        BiomeId = biomeId ?? string.Empty;
        WorldConfigId = worldConfigId ?? string.Empty;
        ThreatLevel = Mathf.Max(1, threatLevel);
        ShipThreatCap = Mathf.Max(1, shipThreatCap);
    }

    public void Clear()
    {
        PlanetId = string.Empty;
        BiomeId = string.Empty;
        WorldConfigId = string.Empty;
        ThreatLevel = 1;
        ShipThreatCap = 1;
    }
}
