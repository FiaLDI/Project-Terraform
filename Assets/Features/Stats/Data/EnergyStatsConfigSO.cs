using UnityEngine;

[CreateAssetMenu(menuName = "Game/Stats/Energy")]
public class EnergyStatsConfigSO : ScriptableObject
{
    public float baseMaxEnergy = 100f;
    public float baseRegen = 8f;
}
