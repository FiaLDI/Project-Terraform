using System.Collections;
using Features.Buffs.Domain;
using Features.Effects.Application;
using Features.Effects.Domain;
using Features.Weapons.Domain;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public sealed class EnemyAttackHandler : MonoBehaviour
{
    [Header("Attack Timing")]
    [SerializeField] private float attackDelay = 0.3f;

    [Header("Effect")]
    [SerializeField] private EffectDefinition effect;

    private bool isAttacking;
    private ProjectileConfig runtimeProjectileConfig;
    private EffectDefinition serializedEffect;
    private float serializedAttackDelay;

    private EntityManager em;
    private EnemyEcsRuntimeBinder binder;

    private void Awake()
    {
        binder = GetComponent<EnemyEcsRuntimeBinder>();
        serializedEffect = effect;
        serializedAttackDelay = attackDelay;

        var world = Unity.Entities.World.DefaultGameObjectInjectionWorld;
        if (world != null)
            em = world.EntityManager;

        // Clients also need the combat config so projectile visuals can be resolved from RPCs.
        if (binder != null && binder.Config != null)
            ApplyCombatConfig(binder.Config.combat);
    }

    public void ApplyCombatConfig(Features.Enemy.Data.EnemyCombatConfigSO combatConfig)
    {
        if (combatConfig == null)
            return;

        attackDelay = combatConfig.attackDelay > 0f ? combatConfig.attackDelay : serializedAttackDelay;

        EffectDefinition baseEffect = HasConfiguredEffect(combatConfig.attackEffect)
            ? combatConfig.attackEffect
            : serializedEffect;

        effect = BuildRuntimeEffectDefinition(baseEffect, combatConfig.attackDamage);
    }

    public void TriggerAttack()
    {
        if (!isAttacking)
            StartCoroutine(AttackRoutine());
    }

    public bool TryGetProjectileVisualShot(out Vector3 spawnPos, out Vector3 hitPoint)
    {
        spawnPos = default;
        hitPoint = default;

        if (!TryResolveAttackGeometry(out _, out spawnPos, out _, out hitPoint))
            return false;

        return TryGetClientProjectileVisualConfig(out _);
    }

    public void PlayProjectileVisual(Vector3 spawnPos, Vector3 hitPoint)
    {
        if (!TryGetClientProjectileVisualConfig(out ProjectileConfig projectileConfig))
            return;

        if (ProjectilePool.Instance == null)
            return;

        Quaternion rotation = projectileConfig.visualType == ProjectileVisualType.Projectile
            ? Quaternion.LookRotation((hitPoint - spawnPos).normalized)
            : Quaternion.identity;

        GameObject go = ProjectilePool.Instance.Get(
            projectileConfig.clientProjectilePrefab,
            spawnPos,
            rotation
        );

        var visual = go != null ? go.GetComponent<IProjectileVisual>() : null;
        visual?.Init(spawnPos, hitPoint, projectileConfig.lifetime);
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
        if (!TryResolveAttackGeometry(out _, out Vector3 origin, out Vector3 dir, out _))
            return;

        IBuffSource source = GetComponentInParent<IBuffSource>();

        var ctx = new EffectContext(
            source,
            null,
            origin,
            dir
        );

        EffectExecutor.Instance?.Execute(effect, ctx);
    }

    private EffectDefinition BuildRuntimeEffectDefinition(EffectDefinition baseEffect, float attackDamage)
    {
        var runtimeEffect = baseEffect;
        float resolvedDamage = Mathf.Max(0f, attackDamage);

        if (resolvedDamage > 0f)
        {
            switch (runtimeEffect.type)
            {
                case EffectType.DealDamage:
                case EffectType.HitscanDamage:
                case EffectType.ChainDamage:
                    runtimeEffect.value = resolvedDamage;
                    break;

                case EffectType.SpawnProjectile:
                    if (runtimeEffect.projectileConfig != null)
                    {
                        if (runtimeProjectileConfig != null)
                            Destroy(runtimeProjectileConfig);

                        runtimeProjectileConfig = Instantiate(runtimeEffect.projectileConfig);
                        runtimeProjectileConfig.damage = resolvedDamage;
                        runtimeProjectileConfig.damageType = runtimeEffect.damageType;
                        OverrideEnemyProjectileHitMask(runtimeProjectileConfig);
                        runtimeEffect.projectileConfig = runtimeProjectileConfig;
                    }
                    break;
            }
        }

        return runtimeEffect;
    }

    private static void OverrideEnemyProjectileHitMask(ProjectileConfig projectileConfig)
    {
        if (projectileConfig == null)
            return;

        int playerMask = LayerMask.GetMask("Player");
        if (playerMask != 0)
            projectileConfig.hitMask = playerMask;
    }

    private static bool HasConfiguredEffect(EffectDefinition effectDefinition)
    {
        if (effectDefinition.value > 0f ||
            effectDefinition.radius > 0f ||
            effectDefinition.projectileConfig != null ||
            effectDefinition.buff != null ||
            effectDefinition.childEffects != null && effectDefinition.childEffects.Length > 0 ||
            !string.IsNullOrWhiteSpace(effectDefinition.prefabId) ||
            !string.IsNullOrWhiteSpace(effectDefinition.impactFxId) ||
            effectDefinition.soundConfig != null)
        {
            return true;
        }

        return effectDefinition.type != default || effectDefinition.targetMode != default;
    }

    private bool TryResolveAttackGeometry(
        out EnemyAI enemyAI,
        out Vector3 origin,
        out Vector3 dir,
        out Vector3 targetPos)
    {
        enemyAI = default;
        origin = default;
        dir = default;
        targetPos = default;

        if (binder == null || binder.Entity == Entity.Null)
            return false;

        if (!em.Exists(binder.Entity))
            return false;

        if (!em.HasComponent<EnemyTarget>(binder.Entity) ||
            !em.HasComponent<EnemyAI>(binder.Entity) ||
            !em.HasComponent<EnemyHasLineOfSight>(binder.Entity))
            return false;

        var enemyTarget = em.GetComponentData<EnemyTarget>(binder.Entity);
        enemyAI = em.GetComponentData<EnemyAI>(binder.Entity);
        var enemyLos = em.GetComponentData<EnemyHasLineOfSight>(binder.Entity);

        if (enemyTarget.Value == Entity.Null || !em.HasComponent<LocalTransform>(enemyTarget.Value))
            return false;

        targetPos = em.GetComponentData<LocalTransform>(enemyTarget.Value).Position;

        if (enemyAI.RequireLOS && !enemyLos.Value)
            return false;

        Vector3 flatOffset = targetPos - transform.position;
        flatOffset.y = 0f;

        if (flatOffset.sqrMagnitude > enemyAI.AttackRange * enemyAI.AttackRange)
            return false;

        dir = targetPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return false;

        dir.Normalize();
        origin = transform.position + dir * 1.2f;
        return true;
    }

    private bool TryGetClientProjectileVisualConfig(out ProjectileConfig projectileConfig)
    {
        projectileConfig = null;

        if (effect.type != EffectType.SpawnProjectile || effect.projectileConfig == null)
            return false;

        projectileConfig = effect.projectileConfig;
        return !projectileConfig.useServerProjectile &&
               projectileConfig.clientProjectilePrefab != null;
    }
}
