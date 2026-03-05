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
    private ProjectileConfig cfg;
    private Rigidbody rb;
    private float lifeTimer;
    private NetworkObject owner;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        rb ??= GetComponent<Rigidbody>();
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

        rb.isKinematic = false;
        rb.useGravity = cfg.useGravity;

        direction.Normalize();

        transform.rotation = Quaternion.LookRotation(direction);

        rb.linearVelocity = direction * cfg.speed;

        lifeTimer = cfg.lifetime;
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

        if ((cfg.hitMask.value & (1 << other.gameObject.layer)) == 0)
            return;

        if (!other.TryGetComponent<IBuffTarget>(out var target))
            return;

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
            targetMode = TargetMode.Self
        };

        EffectExecutor.Instance.Execute(def, ctx);

        RpcPlayHitFx(transform.position, rb.linearVelocity.normalized);

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
