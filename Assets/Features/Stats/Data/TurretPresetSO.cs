using UnityEngine;

[CreateAssetMenu(menuName = "Game/Turret/Preset")]
public class TurretPresetSO : ScriptableObject
{
    public float baseDamageMultiplier = 1f;

    [Header("HP")]
    public float baseHp = 150f;
    public float baseRegen = 0f;
    
    [Header("Rotation")]
    public float rotationSpeed = 10f;

    [Header("Attack")]
    public float baseFireRate = 1f;
}
