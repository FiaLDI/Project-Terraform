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
    private bool waitingForRelease;

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
        burstRemaining = action.burstCount;
        tickTimer = 0f;
        waitingForRelease = action.fireOnRelease;

        if (action.windupTime > 0)
        {
            state = ItemActionState.Windup;
            timer = action.windupTime;
            ItemTickSystem.Register(this);
        }
        else if (action.fireOnRelease)
        {
            state = ItemActionState.Active;
        }
        else
        {
            state = ItemActionState.Active;
            FireActive();

            if (state != ItemActionState.Idle)
                ItemTickSystem.Register(this);
        }
    }

    // ======================================================

    public void StopUse()
    {
        if (action.fireOnRelease)
        {
            if (state == ItemActionState.Active)
            {
                waitingForRelease = false;
                FireActive();

                if (state != ItemActionState.Idle)
                    ItemTickSystem.Register(this);

                return;
            }
        }

        waitingForRelease = false;
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
        {
            state = ItemActionState.Active;

            if (action.fireOnRelease)
            {
                ItemTickSystem.Unregister(this);
                return;
            }

            FireActive();
        }
    }

    // ======================================================

    private void TickActive(float dt)
    {
        if (waitingForRelease)
            return;

        tickTimer -= dt;

        if (tickTimer > 0f)
            return;

        FireActive();
    }

    private void FireActive()
    {
        ExecuteEffects();

        if (state == ItemActionState.Idle)
            return;

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
        {
            state = ItemActionState.Idle;
            waitingForRelease = false;
            ItemTickSystem.Unregister(this);
            return;
        }

        state = ItemActionState.Cooldown;
        waitingForRelease = false;
        timer = action.cooldown;
        ItemTickSystem.Register(this);
    }

    private void TickCooldown(float dt)
    {
        timer -= dt;

        if (timer <= 0f)
        {
            state = ItemActionState.Idle;
            waitingForRelease = false;
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
            TryAdjustDirectionForNearObstacle(fireOrigin, ref dir);
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

    private void TryAdjustDirectionForNearObstacle(Vector3 fireOrigin, ref Vector3 dir)
    {
        if (dir.sqrMagnitude <= 0.0001f)
            return;

        var hits = Physics.RaycastAll(
            fireOrigin,
            dir,
            1.0f,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        if (hits == null || hits.Length == 0)
            return;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        var sourceComponent = source as Component;
        Transform sourceRoot = sourceComponent != null ? sourceComponent.transform : null;

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            if (hit.collider == null)
                continue;

            if (sourceRoot != null && hit.collider.transform.IsChildOf(sourceRoot))
                continue;

            Vector3 delta = hit.point - fireOrigin;
            if (delta.sqrMagnitude <= 0.0001f)
                continue;

            dir = delta.normalized;
            return;
        }
    }
}
