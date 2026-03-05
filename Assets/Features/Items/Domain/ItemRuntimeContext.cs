using UnityEngine;
using Features.Effects.Domain;
using Features.Effects.Application;
using Features.Buffs.Domain;

public class ItemRuntimeContext : IItemTickable
{
    private readonly IBuffSource source;
    private readonly ItemActionDefinition action;

    private Vector3 origin;
    private Vector3 direction;

    private ItemActionState state = ItemActionState.Idle;

    private float timer;
    private float tickTimer;

    private int burstRemaining;

    public ItemRuntimeContext(
        IBuffSource source,
        ItemActionDefinition action)
    {
        this.source = source;
        this.action = action;
    }

    // ======================================================
    // START
    // ======================================================

    public void StartUse(Vector3 origin, Vector3 direction)
    {
        this.origin = origin;
        this.direction = direction;

        if (action.windupTime > 0)
        {
            state = ItemActionState.Windup;
            timer = action.windupTime;
        }
        else
        {
            state = ItemActionState.Active;
        }

        burstRemaining = action.burstCount;

        tickTimer = 0;

        ItemTickSystem.Register(this);
    }

    // ======================================================

    public void StopUse()
    {
        state = ItemActionState.Idle;

        ItemTickSystem.Unregister(this);
    }

    // ======================================================

    public void UpdateAim(Vector3 origin, Vector3 direction)
    {
        this.origin = origin;
        this.direction = direction;
    }

    // ======================================================
    // TICK
    // ======================================================

    public void ServerTick(float dt)
    {
        switch (state)
        {
            case ItemActionState.Windup:
                TickWindup(dt);
                break;

            case ItemActionState.Active:
                TickActive(dt);
                break;

            case ItemActionState.Cooldown:
                TickCooldown(dt);
                break;
        }
    }

    // ======================================================

    private void TickWindup(float dt)
    {
        timer -= dt;

        if (timer <= 0)
        {
            state = ItemActionState.Active;
        }
    }

    // ======================================================

    private void TickActive(float dt)
    {
        tickTimer -= dt;

        if (tickTimer > 0)
            return;

        ExecuteEffects();

        if (action.burstCount > 0)
        {
            burstRemaining--;

            if (burstRemaining <= 0)
            {
                StartCooldown();
                return;
            }

            tickTimer = action.burstInterval;
        }
        else
        {
            tickTimer = action.tickInterval;
        }
    }

    // ======================================================

    private void StartCooldown()
    {
        if (action.cooldown <= 0)
            return;

        state = ItemActionState.Cooldown;
        timer = action.cooldown;
    }

    private void TickCooldown(float dt)
    {
        timer -= dt;

        if (timer <= 0)
        {
            state = ItemActionState.Idle;
            ItemTickSystem.Unregister(this);
        }
    }

    // ======================================================

    private void ExecuteEffects()
    {
        if (action.effects == null)
            return;

        var ctx = new EffectContext(
            source,
            null,
            origin,
            direction
        );

        foreach (var def in action.effects)
            EffectExecutor.Instance.Execute(def, ctx);
    }
}
