using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class ShipLevelConfig
{
    [Min(1)]
    public int shipLevel = 1;

    [Header("Unlocks")]
    public List<string> unlockedPlanetIds = new List<string>();

    [Header("Caps")]
    [Min(1)]
    public int maxThreatLevel = 3;

    [Header("Upgrade Requirements")]
    [Min(1)]
    public int requiredClassLevel = 1;

    public List<string> requiredCompletedPlanetIds = new List<string>();
    public List<ShipUpgradeCostConfig> upgradeCosts = new List<ShipUpgradeCostConfig>();
}

[System.Serializable]
public sealed class ShipUpgradeCostConfig
{
    public string itemId;

    [Min(1)]
    public int amount = 1;
}
