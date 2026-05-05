using System;
using System.Collections.Generic;

[Serializable]
public sealed class CampaignSaveData
{
    public List<ExpeditionSaveData> expeditions = new List<ExpeditionSaveData>();
    public string activeExpeditionId = string.Empty;
    public int version = 1;
}
