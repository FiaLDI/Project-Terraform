public static class CampaignPlanetMissionRuntimeState
{
    public static string ActivePlanetId { get; private set; }
    public static string ActiveMissionId { get; private set; }

    public static bool HasActiveMission =>
        !string.IsNullOrWhiteSpace(ActivePlanetId) &&
        !string.IsNullOrWhiteSpace(ActiveMissionId);

    public static void Set(string planetId, string missionId)
    {
        ActivePlanetId = planetId ?? string.Empty;
        ActiveMissionId = missionId ?? string.Empty;
    }

    public static void Clear()
    {
        ActivePlanetId = string.Empty;
        ActiveMissionId = string.Empty;
    }
}
