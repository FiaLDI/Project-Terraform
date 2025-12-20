using UnityEngine;

[CreateAssetMenu(menuName = "Game/Stats/Combat")]
public class CombatStatsConfigSO : ScriptableObject
{
    [Header("Базовый множитель урона")]
    public float baseDamageMultiplier = 1f;
}
