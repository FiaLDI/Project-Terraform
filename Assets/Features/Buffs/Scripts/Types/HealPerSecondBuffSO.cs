using UnityEngine;
using System.Runtime.CompilerServices;


[CreateAssetMenu(menuName = "Game/Buffs/Heal Per Second")]
public class HealPerSecondBuffSO : BuffSO
{
    [Header("Healing")]
    public float healPerSecond = 10f;

    private readonly ConditionalWeakTable<BuffInstance, TimerHolder> timers = new();

    public override void OnApply(BuffInstance instance)
    {
        timers.Add(instance, new TimerHolder());
    }

    public override void OnTick(BuffInstance instance, float dt)
    {
        if (instance.Target == null) return;

        if (!instance.Target.GameObject.TryGetComponent<PlayerHealth>(out var health))
            return;

        var timer = timers.GetOrCreateValue(instance);
        timer.accum += dt;

        if (timer.accum >= 1f)
        {
            health.Heal(healPerSecond);
            timer.accum = 0f;
        }
    }

    public override void OnExpire(BuffInstance instance)
    {
        
    }

    private class TimerHolder
    {
        public float accum = 0f;
    }
}
