using Features.Effects.Domain;
using Features.Buffs.Domain;
using Features.Items.UnityIntegration;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;
using FishNet.Object;
using Unity.Entities;
using UnityEngine;

namespace Features.Effects.Application
{
    public sealed class DealDamageEffect : IEffect
    {
        private readonly float _value;
        private readonly DamageType _type;

        public DealDamageEffect(float value, DamageType type)
        {
            _value = value;
            _type = type;
        }

        public void Apply(EffectContext context)
        {
            
            if (context.Targets == null)
                return;

            foreach (var t in context.Targets)
            {
                if (t?.BuffSystem == null || !t.IsReady)
                    continue;

                var statsOwner = t.BuffSystem.GetComponentInParent<IStatsOwner>();

                if (statsOwner == null || !statsOwner.IsReady)
                    continue;

                var stats = statsOwner.Facade;
                stats?.Health?.Damage(_value);

                var enemy = t.BuffSystem.GetComponentInParent<EnemyStats>();

                if (enemy != null && context.Source != null)
                {
                    enemy.RegisterAttacker(context.Source);

                    if (TryResolveEnemyEntity(t, out var enemyEntity, out var em) &&
                        TryResolveSourceEntity(context.Source, out var sourceEntity))
                    {
                        EnemyAggroUtility.AddDamageEvent(em, enemyEntity, sourceEntity, _value);
                    }
                }
            }
        }

        private static bool TryResolveEnemyEntity(IBuffTarget target, out Entity enemyEntity, out EntityManager em)
        {
            enemyEntity = Entity.Null;
            em = default;

            if (target?.BuffSystem == null)
                return false;

            var binder = target.BuffSystem.GetComponentInParent<EnemyEcsRuntimeBinder>();
            if (binder == null)
                return false;

            var world = Unity.Entities.World.DefaultGameObjectInjectionWorld;
            if (world == null)
                return false;

            em = world.EntityManager;
            enemyEntity = binder.Entity;
            return enemyEntity != Entity.Null && em.Exists(enemyEntity);
        }

        private static bool TryResolveSourceEntity(IBuffSource source, out Entity sourceEntity)
        {
            sourceEntity = Entity.Null;

            if (source == null)
                return false;

            if (source is NetworkBehaviour networkBehaviour &&
                networkBehaviour.Owner != null &&
                PlayerRegistryECS.TryGet(networkBehaviour.Owner.ClientId, out sourceEntity))
                return true;

            if (source is ItemRuntimeSource itemSource)
                return TryResolveSourceEntity(itemSource.OwnerSource, out sourceEntity);

            if (source is RuntimeBuffSource runtimeSource)
            {
                if (runtimeSource.Owner is IBuffSource ownerSource)
                    return TryResolveSourceEntity(ownerSource, out sourceEntity);

                if (runtimeSource.Owner is Component ownerComponent)
                    return TryResolveSourceEntityFromComponent(ownerComponent, out sourceEntity);
            }

            if (source is Component component)
                return TryResolveSourceEntityFromComponent(component, out sourceEntity);

            return false;
        }

        private static bool TryResolveSourceEntityFromComponent(Component component, out Entity sourceEntity)
        {
            sourceEntity = Entity.Null;

            if (component == null)
                return false;

            var networkBehaviour = component as NetworkBehaviour ?? component.GetComponentInParent<NetworkBehaviour>();
            if (networkBehaviour == null || networkBehaviour.Owner == null)
                return false;

            return PlayerRegistryECS.TryGet(networkBehaviour.Owner.ClientId, out sourceEntity);
        }
    }
}
