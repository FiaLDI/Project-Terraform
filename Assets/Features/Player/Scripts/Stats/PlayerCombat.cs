using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Base combat stats")]
    public float baseDamageMultiplier = 1f;

    // Base + buffs
    public float BonusDamage { get; private set; } = 0f;

    public float DamageMultiplier => baseDamageMultiplier + BonusDamage;

    public float ApplyDamageModifiers(float baseDamage)
    {
        return baseDamage * DamageMultiplier;
    }

    public void AddDamage(float amount)
    {
        BonusDamage += amount;
    }

    public void RemoveDamage(float amount)
    {
        BonusDamage -= amount;
        if (BonusDamage < 0f) BonusDamage = 0f;
    }

    public void SetBaseDamage(float amount)
    {
        baseDamageMultiplier = amount;
    }
}
