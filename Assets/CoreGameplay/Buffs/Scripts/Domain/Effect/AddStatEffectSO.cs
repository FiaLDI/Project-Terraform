using Features.Buffs.Domain;
using Features.Stats.Domain;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Buff/Effects/Add Stat")]
public sealed class AddStatEffectSO : BuffEffectSO
{
    [StatKey]
    public string statId;

    public float value;

    public override void Apply(IStatsFacade stats)
    {
        stats.TryAdd(new StatKey(statId), value);
    }

    public override void Expire(IStatsFacade stats)
    {
        stats.TryAdd(new StatKey(statId), -value);
    }
}

