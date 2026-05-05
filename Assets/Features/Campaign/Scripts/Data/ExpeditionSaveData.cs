using System;
using System.Collections.Generic;

[Serializable]
public sealed class ExpeditionSaveData
{
    public string expeditionId;
    public string displayName;

    public int shipLevel = 1;
    public string activePlanetId;

    public List<PlanetProgressData> planets = new List<PlanetProgressData>();

    public string lastPlayedUtc;
}
