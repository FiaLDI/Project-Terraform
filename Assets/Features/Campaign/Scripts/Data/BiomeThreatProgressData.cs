using System;
using System.Collections.Generic;

[Serializable]
public sealed class BiomeThreatProgressData
{
    public string biomeId;

    public int maxUnlockedThreatLevel = 1;
    public List<int> completedThreatLevels = new List<int>();
}
