using FishNet.Object;
using UnityEngine;
using Features.Combat.Application;
using Features.Combat.Domain;
using Features.Weapons.Domain;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public sealed class ProjectileNetwork : NetworkBehaviour
{
    private ProjectileConfig cfg;
    private CombatService combat;

    private Rigidbody rb;
    private float lifeTimer;

    private int ownerObjectId = -1;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        rb ??= GetComponent<Rigidbody>();
    }

    [Server]
    public void InitServer(ProjectileConfig config, NetworkObject owner)
    {
        cfg = config;
        combat = CombatServiceProvider.Service;

        ownerObjectId = owner != null ? owner.ObjectId : -1;

        rb.isKinematic = false;
        rb.useGravity = cfg.useGravity;
        rb.linearVelocity = transform.forward * cfg.speed;

        lifeTimer = cfg.lifetime;

        // по желанию: игнор коллизии со всеми коллайдерами владельца
        if (owner != null)
        {
            var ownerCols = owner.GetComponentsInChildren<Collider>(true);
            var myCol = GetComponent<Collider>();
            foreach (var c in ownerCols)
                Physics.IgnoreCollision(myCol, c, true);
        }
    }

    private void Update()
    {
        if (!IsServerInitialized)
            return;

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
            Despawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServerInitialized || cfg == null)
            return;

        // маска попаданий
        if ((cfg.hitMask.value & (1 << other.gameObject.layer)) == 0)
            return;

        // защита от самопопадания по NetworkObject
        if (ownerObjectId != -1)
        {
            var otherNet = other.GetComponentInParent<FishNet.Object.NetworkObject>();
            if (otherNet != null && otherNet.ObjectId == ownerObjectId)
                return;
        }

        var target = other.GetComponentInParent<IDamageable>();
        if (target != null)
        {
            var dir = rb.linearVelocity.sqrMagnitude > 0.0001f ? rb.linearVelocity.normalized : transform.forward;

            var hit = new HitInfo
            {
                damage = cfg.damage,
                type = cfg.damageType,
                point = transform.position,
                direction = dir
            };

            // ✅ ТОЛЬКО ЭТО. НИКАКОГО target.TakeDamage ниже!
            combat.ApplyDamage(target, hit, DamageModifiers.Default);
        }

        RpcPlayHitFx(transform.position, rb.linearVelocity.sqrMagnitude > 0.0001f ? rb.linearVelocity.normalized : transform.forward);

        if (cfg.destroyOnHit)
            Despawn();
    }

    [ObserversRpc]
    private void RpcPlayHitFx(Vector3 pos, Vector3 forward)
    {
        if (cfg != null && cfg.hitEffect != null)
            Instantiate(cfg.hitEffect, pos, Quaternion.LookRotation(forward));
    }
}
