using UnityEngine;
using Features.Effects.Domain;
using Features.Effects.Application;
using Features.Buffs.Domain;

public class ItemRuntimeContext : IItemTickable
{
    private readonly IBuffSource source;
    private readonly ItemActionDefinition action;

    private Vector3 origin;
    private Vector3 targetPoint;
    private Vector3 fallbackDirection;

    private bool hasTargetPoint;

    private ItemActionState state = ItemActionState.Idle;

    private float timer;
    private float tickTimer;

    private int burstRemaining;

    public System.Action<Vector3, Vector3> OnFire;

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

    public void StartUse(Vector3 hitPoint)
    {
        targetPoint = hitPoint;
        hasTargetPoint = true;

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
        tickTimer = 0f;

        ItemTickSystem.Register(this);
    }

    // ======================================================

    public void StopUse()
    {
        state = ItemActionState.Idle;
        ItemTickSystem.Unregister(this);
    }

    // ======================================================
    // AIM UPDATE (универсальный)
    // ======================================================

    public void UpdateAim(Vector3 fireOrigin, Vector3 directionOrHitPoint, bool isHitPoint)
    {
        this.origin = fireOrigin;

        if (isHitPoint)
        {
            targetPoint = directionOrHitPoint;
            hasTargetPoint = true;
        }
        else
        {
            fallbackDirection = directionOrHitPoint.normalized;
            hasTargetPoint = false;
        }
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

        if (timer <= 0f)
            state = ItemActionState.Active;
    }

    // ======================================================

    private void TickActive(float dt)
    {
        tickTimer -= dt;

        if (tickTimer > 0f)
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
        if (action.cooldown <= 0f)
            return;

        state = ItemActionState.Cooldown;
        timer = action.cooldown;
    }

    private void TickCooldown(float dt)
    {
        timer -= dt;

        if (timer <= 0f)
        {
            state = ItemActionState.Idle;
            ItemTickSystem.Unregister(this);
        }
    }

    private void ExecuteEffects()
    {
        if (action.effects == null)
            return;

        Vector3 fireOrigin = origin;

        Vector3 dir;

        if (hasTargetPoint)
        {
            dir = (targetPoint - fireOrigin).normalized;

            if (Physics.Raycast(fireOrigin, dir, out var hit, 1.0f))
            {
                dir = (hit.point - fireOrigin).normalized;
            }
        }
        else
        {
            dir = fallbackDirection;
        }

       EffectContext ctx;

        if (hasTargetPoint)
        {
            ctx = new HitEffectContext(
                source,
                null,
                fireOrigin,
                dir,
                targetPoint,
                -dir
            );
        }
        else
        {
            ctx = new EffectContext(
                source,
                null,
                fireOrigin,
                dir
            );
        }

        foreach (var def in action.effects)
            EffectExecutor.Instance.Execute(def, ctx);

    #if UNITY_EDITOR
        Debug.DrawRay(fireOrigin, dir * 3f, Color.red, 0.1f);
    #endif

        OnFire?.Invoke(fireOrigin, dir);
    }
}
