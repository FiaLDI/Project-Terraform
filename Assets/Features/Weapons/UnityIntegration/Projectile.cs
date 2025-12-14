using UnityEngine;
using Features.Combat.Application;
using Features.Combat.Domain;
using Features.Weapons.Domain;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour, IProjectile
{
    private ProjectileConfig config;
    private CombatService combat;

    private float lifeTimer;
    private Rigidbody rb;

    // ======================================================
    // SETUP
    // ======================================================

    public void Setup(ProjectileConfig config, CombatService combat)
    {
        this.config = config;
        this.combat = combat;

        lifeTimer = config.lifetime;

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = config.useGravity;
        rb.linearVelocity = transform.forward * config.speed;
    }

    // ======================================================
    // UPDATE (ТОЛЬКО LIFETIME)
    // ======================================================

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    // ======================================================
    // COLLISION
    // ======================================================

    private void OnTriggerEnter(Collider other)
    {
        // === ignore self / weapon / player ===
        if ((config.hitMask.value & (1 << other.gameObject.layer)) == 0)
            return;

        var target = other.GetComponentInParent<IDamageable>();
        if (target != null)
        {
            HitInfo hit = new HitInfo
            {
                damage = config.damage,
                type = config.damageType,
                point = transform.position,
                direction = rb.linearVelocity.normalized
            };

            combat.ApplyDamage(
                target,
                hit,
                DamageModifiers.Default
            );

            target.TakeDamage(config.damage, config.damageType);
        }

        if (config.hitEffect != null)
        {
            Instantiate(
                config.hitEffect,
                transform.position,
                Quaternion.LookRotation(rb.linearVelocity)
            );
        }

        if (config.destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}
