using FishNet.Object;
using UnityEngine;
using Features.Effects.Domain;
using Features.Effects.Application;
using Features.Buffs.Domain;
using Features.Weapons.Domain;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public sealed class ProjectileNetwork : NetworkBehaviour
{
    private static readonly RaycastHit[] SweepHits = new RaycastHit[32];
    private static readonly Collider[] OverlapHits = new Collider[32];

    private ProjectileConfig cfg;
    private Rigidbody rb;
    private Collider ownCollider;
    private float lifeTimer;
    private NetworkObject owner;
    private bool exploded;
    private Vector3 previousPosition;
    private float contactRadius = 0.1f;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        rb ??= GetComponent<Rigidbody>();
        ownCollider ??= GetComponent<Collider>();
    }

    [Server]
    public void InitServer(
        ProjectileConfig config,
        NetworkObject ownerObj,
        Vector3 direction)
    {
        cfg = config;
        owner = ownerObj;

        rb ??= GetComponent<Rigidbody>();
        ownCollider ??= GetComponent<Collider>();

        rb.isKinematic = false;
        rb.useGravity = cfg.useGravity;
        rb.detectCollisions = true;

        ConfigurePhysics();

        direction.Normalize();

        transform.rotation = Quaternion.LookRotation(direction);

        rb.linearVelocity = direction * cfg.speed;

        lifeTimer = cfg.lifetime;
        exploded = false;
        previousPosition = transform.position;
        contactRadius = ResolveContactRadius();

        IgnoreOwnerCollisions();
    }

    private void Update()
    {
        if (!IsServerInitialized)
            return;

        if (HasExplosion() && TryExplodeBySweep())
            return;

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            if (HasExplosion())
                Explode(transform.position, Vector3.up);
            else
                Despawn();
        }

        previousPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Projectile] Hit: {other.name}");
        if (!IsServerInitialized || cfg == null)
            return;

        if (ShouldIgnoreCollider(other))
            return;

        if (TryExplodeOnContact(other, transform.position, ResolveFallbackNormal()))
            return;

        if ((cfg.hitMask.value & (1 << other.gameObject.layer)) == 0)
            return;

        if (!other.TryGetComponent<IBuffTarget>(out var target))
        {
            target = other.GetComponentInParent<IBuffTarget>();
            if (target == null)
                return;
        }

        Debug.Log($"[Projectile] none ERRORS");

        var ctx = new EffectContext(
            owner != null ? owner.GetComponent<IBuffSource>() : null,
            new[] { target },
            transform.position,
            rb.linearVelocity.normalized
        );

        var def = new EffectDefinition
        {
            type = EffectType.DealDamage,
            value = cfg.damage,
            targetMode = TargetMode.Explicit
        };

        EffectExecutor.Instance.Execute(def, ctx);

        RpcPlayHitFx(transform.position, rb.linearVelocity.normalized);

        if (cfg.destroyOnHit)
            Despawn();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServerInitialized || cfg == null || collision == null)
            return;

        var contact = collision.contactCount > 0
            ? collision.GetContact(0)
            : default;

        Vector3 position = collision.contactCount > 0
            ? contact.point
            : transform.position;

        Vector3 normal = collision.contactCount > 0
            ? contact.normal
            : ResolveFallbackNormal();

        TryExplodeOnContact(collision.collider, position, normal);
    }

    private bool TryExplodeOnContact(Collider other, Vector3 position, Vector3 normal)
    {
        if (!HasExplosion())
            return false;

        if (IsOwnerCollider(other))
            return true;

        Explode(position, normal);
        return true;
    }

    private void ConfigurePhysics()
    {
        if (!HasExplosion())
            return;

        if (ownCollider != null)
            ownCollider.isTrigger = false;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private bool HasExplosion()
    {
        return cfg != null && cfg.explosionRadius > 0f;
    }

    private bool TryExplodeBySweep()
    {
        Vector3 current = transform.position;
        Vector3 delta = current - previousPosition;
        float distance = delta.magnitude;

        if (distance > 0.001f)
        {
            Vector3 direction = delta / distance;
            int hitCount = Physics.SphereCastNonAlloc(
                previousPosition,
                contactRadius,
                direction,
                SweepHits,
                distance,
                ~0,
                QueryTriggerInteraction.Collide
            );

            if (TryResolveExplosionHit(hitCount, out var hit))
            {
                Explode(hit.point, hit.normal);
                return true;
            }
        }

        int overlapCount = Physics.OverlapSphereNonAlloc(
            current,
            contactRadius,
            OverlapHits,
            ~0,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < overlapCount; i++)
        {
            var col = OverlapHits[i];
            if (ShouldIgnoreCollider(col))
                continue;

            Explode(current, ResolveFallbackNormal());
            return true;
        }

        return false;
    }

    private bool TryResolveExplosionHit(int hitCount, out RaycastHit best)
    {
        best = default;
        bool found = false;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            var hit = SweepHits[i];
            if (ShouldIgnoreCollider(hit.collider))
                continue;

            if (hit.distance < bestDistance)
            {
                bestDistance = hit.distance;
                best = hit;
                found = true;
            }
        }

        return found;
    }

    private bool IsOwnerCollider(Collider other)
    {
        if (owner == null || other == null)
            return false;

        var otherNetworkObject = other.GetComponentInParent<NetworkObject>();
        if (otherNetworkObject != null && otherNetworkObject == owner)
            return true;

        return other.transform == owner.transform ||
               other.transform.IsChildOf(owner.transform);
    }

    private bool ShouldIgnoreCollider(Collider other)
    {
        if (other == null)
            return true;

        if (other == ownCollider || other.transform.IsChildOf(transform))
            return true;

        return IsOwnerCollider(other);
    }

    private void IgnoreOwnerCollisions()
    {
        if (owner == null || ownCollider == null)
            return;

        var ownerColliders = owner.GetComponentsInChildren<Collider>();
        for (int i = 0; i < ownerColliders.Length; i++)
        {
            var ownerCollider = ownerColliders[i];
            if (ownerCollider == null || ownerCollider == ownCollider)
                continue;

            Physics.IgnoreCollision(ownCollider, ownerCollider, true);
        }
    }

    private float ResolveContactRadius()
    {
        if (ownCollider == null)
            return 0.1f;

        var extents = ownCollider.bounds.extents;
        float radius = HasExplosion()
            ? Mathf.Max(extents.x, extents.y, extents.z)
            : Mathf.Min(extents.x, extents.y, extents.z);

        return Mathf.Clamp(radius, 0.05f, 0.5f);
    }

    private Vector3 ResolveFallbackNormal()
    {
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.0001f)
            return -rb.linearVelocity.normalized;

        return Vector3.up;
    }

    private void Explode(Vector3 position, Vector3 normal)
    {
        if (exploded)
            return;

        exploded = true;

        var source = owner != null ? owner.GetComponent<IBuffSource>() : null;

        var ctx = new EffectContext(
            source,
            null,
            position,
            Vector3.zero
        );

        var damageMask = cfg.explosionLayerMask.value != 0
            ? cfg.explosionLayerMask
            : cfg.hitMask;

        EffectExecutor.Instance.Execute(
            new EffectDefinition
            {
                type = EffectType.DealDamage,
                targetMode = TargetMode.Area,
                radius = cfg.explosionRadius,
                layerMask = damageMask,
                ownership = cfg.explosionOwnership,
                value = cfg.damage,
                damageType = cfg.damageType
            },
            ctx
        );

        if (!string.IsNullOrEmpty(cfg.explosionImpactFxId))
            ImpactFxDispatcher.Instance?.ServerSpawn(position, normal, cfg.explosionImpactFxId);

        if (cfg.explosionSound != null)
            ImpactFxDispatcher.Instance?.ServerPlaySound(cfg.explosionSound, position);

        Despawn();
    }

    [ObserversRpc]
    private void RpcPlayHitFx(Vector3 pos, Vector3 forward)
    {
        if (cfg != null && cfg.hitEffect != null)
            Instantiate(cfg.hitEffect, pos, Quaternion.LookRotation(forward));
    }
}
