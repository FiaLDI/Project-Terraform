using Biomes.Data;
using UnityEngine;

[System.Serializable]
public sealed class PlanetConfig
{
    [Header("ID")]
    public string planetId;

    [Header("Info")]
    public string displayName;

    [TextArea]
    public string description;

    [Header("Access")]
    [Min(1)]
    public int requiredShipLevel = 1;

    [Header("World")]
    public WorldConfig worldConfig;

    [Header("Planet Mission")]
    public string planetMissionId;
    public string planetMissionSceneName;

    [Min(1)]
    public int planetMissionUnlockThreatLevel = 2;
}
