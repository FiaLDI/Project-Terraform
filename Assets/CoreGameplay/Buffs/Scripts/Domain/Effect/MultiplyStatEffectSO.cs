using UnityEngine;
using Features.Buffs.Domain;
using Features.Stats.Domain;

[CreateAssetMenu(menuName = "Game/Buff/Effects/Multiply Stat")]
public sealed class MultiplyStatEffectSO : BuffEffectSO
{
    [StatKey]
    [SerializeField] private string statId;

    [SerializeField] private float multiplier = 1f;
    public string StatId => statId;
    public float Multiplier => multiplier;

    public override void Apply(IStatsFacade stats)
    {
        if (string.IsNullOrEmpty(statId) || multiplier == 1f)
            return;

        stats.TryMultiply(new StatKey(statId), multiplier);
    }

    public override void Expire(IStatsFacade stats)
    {
        if (string.IsNullOrEmpty(statId) || multiplier == 1f)
            return;

        stats.TryMultiply(new StatKey(statId), 1f / multiplier);
    }
}
