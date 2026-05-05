using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CampaignCatalog", menuName = "Game/Campaign/Campaign Catalog")]
public sealed class CampaignCatalogSO : ScriptableObject
{
    public List<ShipLevelConfig> shipLevels = new List<ShipLevelConfig>();
    public List<PlanetConfig> planets = new List<PlanetConfig>();
}
