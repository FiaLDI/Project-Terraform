using Features.Buffs.Domain;
using Features.Stats.Domain;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buff/Effects/Heal Over Time")]
public sealed class HealOverTimeEffectSO : BuffEffectSO
{
    [SerializeField] private float healPerSecond;

    public override void Apply(IStatsFacade stats)
    {
        // ничего — эффект чисто tick-based
    }

    public override void Tick(IStatsFacade stats, float dt)
    {
        if (stats?.Health == null)
            return;

        stats.Health.Heal(healPerSecond * dt);
    }

    public override void Expire(IStatsFacade stats)
    {
        // ничего
    }
}
