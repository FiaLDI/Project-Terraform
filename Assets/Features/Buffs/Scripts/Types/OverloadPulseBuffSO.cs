using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buff/Overload Pulse")]
public class OverloadPulseBuffSO : BuffSO
{
    [Header("Stats")]
    public float speedMultiplier = 1.2f;
    public float damageBonus = 0.15f;

    public override void OnApply(BuffInstance instance)
    {
        var go = instance.Target.GameObject;

        // --- Movement Speed Multiplier ---
        if (go.TryGetComponent<PlayerMovementStats>(out var movement))
            movement.AddSpeedMultiplier(speedMultiplier);

        // --- Damage Bonus ---
        if (go.TryGetComponent<PlayerCombat>(out var combat))
            combat.AddDamage(damageBonus);
    }

    public override void OnExpire(BuffInstance instance)
    {
        var go = instance.Target.GameObject;

        // --- Movement Speed Multiplier ---
        if (go.TryGetComponent<PlayerMovementStats>(out var movement))
            movement.RemoveSpeedMultiplier(speedMultiplier);

        // --- Damage Bonus ---
        if (go.TryGetComponent<PlayerCombat>(out var combat))
            combat.RemoveDamage(damageBonus);
    }
}
