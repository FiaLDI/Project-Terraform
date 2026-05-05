using System;
using System.Collections.Generic;

[Serializable]
public sealed class PlanetProgressData
{
    public string planetId;

    public bool isPlanetMissionUnlocked;
    public bool isPlanetMissionCompleted;

    public List<BiomeThreatProgressData> biomeThreats = new List<BiomeThreatProgressData>();
}
