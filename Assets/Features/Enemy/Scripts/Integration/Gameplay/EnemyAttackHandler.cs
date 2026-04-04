using UnityEngine;
using System.Collections;
using Features.Effects.Domain;
using Features.Effects.Application;
using Features.Buffs.Domain;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public sealed class EnemyAttackHandler : MonoBehaviour
{
    [Header("Attack Timing")]
    [SerializeField] private float attackDelay = 0.3f;

    [Header("Effect")]
    [SerializeField] private EffectDefinition effect;

    private bool isAttacking;

    private EntityManager em;
    private EnemyEcsRuntimeBinder binder;

    private void Awake()
    {
        binder = GetComponent<EnemyEcsRuntimeBinder>();

        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null)
            em = world.EntityManager;
    }

    public void TriggerAttack()
    {
        if (!isAttacking)
            StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        yield return new WaitForSeconds(attackDelay);

        DealDamage();

        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
    }

    private void DealDamage()
    {
        if (binder == null || binder.Entity == Entity.Null)
            return;

        if (!em.Exists(binder.Entity))
            return;

        if (!em.HasComponent<EnemyTarget>(binder.Entity))
            return;

        var enemyTarget = em.GetComponentData<EnemyTarget>(binder.Entity);

        if (enemyTarget.Value == Entity.Null)
            return;

        if (!em.HasComponent<LocalTransform>(enemyTarget.Value))
            return;

        float3 targetPos3 =
            em.GetComponentData<LocalTransform>(enemyTarget.Value).Position;

        Vector3 targetPos = targetPos3;

        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        dir.Normalize();

        Vector3 origin = transform.position + dir * 1.2f;

        IBuffSource source = GetComponentInParent<IBuffSource>(); // 👈 проще

        var ctx = new EffectContext(
            source,
            null,
            origin,
            dir
        );

        var targets = TargetResolver.Resolve(effect, ctx);

        if (targets.Length == 0)
            return;

        var newCtx = new EffectContext(
            ctx.Source,
            targets,
            ctx.Origin,
            ctx.Direction
        );

        var effectInstance = EffectFactory.Create(effect);
        effectInstance?.Apply(newCtx);
    }
}
