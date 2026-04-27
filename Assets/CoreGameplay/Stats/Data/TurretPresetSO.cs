using UnityEngine;

[CreateAssetMenu(menuName = "Game/Turret/Preset")]
public class TurretPresetSO : ScriptableObject
{
    [Header("Legacy")]
    public float baseDamageMultiplier = 1f;

    [Header("HP")]
    public float baseHp = 150f;
    public float baseRegen = 0f;
    
    [Header("Rotation")]
    public float rotationSpeed = 10f;

    [Header("Attack")]
    public float baseFireRate = 1f;

    [Header("Combat")]
    public CombatBlock combat = new CombatBlock();

    [System.Serializable]
    public class CombatBlock
    {
        public float baseDamage = 1f;
        public float baseDamageMultiplier = 1f;
        public float baseFireRate = 5f;
        public float baseSpread = 2f;
        public float baseAimSpread = 0.5f;
        public float baseRange = 100f;
        public float baseRecoil = 1f;
        public int baseMagazineSize = 30;
    }
}
