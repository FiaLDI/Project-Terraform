using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Effects.Domain;
using UnityEngine;

namespace Features.Effects.Application
{
    public static class EffectFactory
    {
        public static IEffect Create(EffectDefinition def)
        {
            return def.type switch
            {
                EffectType.DealDamage =>
                    new DealDamageEffect(def.value, def.damageType),

                EffectType.HitscanDamage =>
                    new HitscanDamageEffect(
                        def.value,
                        def.radius,
                        def.layerMask,
                        def.damageType
                    ),

                EffectType.HealInstant =>
                    new HealInstantEffect(def.value),

                EffectType.ApplyBuff =>
                    new ApplyBuffEffect(def.buff),

                EffectType.RemoveBuffSource =>
                    new RemoveBuffSourceEffect(
                        def.onlySpecificBuff,
                        def.buffId
                    ),

                EffectType.SpawnPrefab =>
                    new SpawnPrefabEffect(
                        def.prefabId,
                        def.lifetime,
                        def.useSourcePosition
                    ),

                EffectType.MineNetworkResource =>
                    new MineNetworkResourceEffect(
                        def.value,
                        def.radius,
                        def.layerMask
                    ),

                EffectType.Continuous =>
                    new ContinuousEffect(def.tickInterval, def.childEffects),

                EffectType.StopContinuous =>
                    new StopContinuousEffect(),

                EffectType.Scan =>
                    new ScanEffect(def.value),
                
            EffectType.ScanResourceEffect =>
                    new ScanResourceEffect(
                        def.prefabId,
                        def.radius,
                        def.layerMask,
                        def.lifetime,
                        def.heightOffset
                    ),

                EffectType.SpawnProjectile =>
                    new SpawnProjectileEffect(
                        def.projectileConfig
                    ),
                
                EffectType.SpawnImpact =>
                    new SpawnImpactEffect(
                        def.impactFxId
                    ),

                EffectType.PlaySound =>
                    new PlaySoundEffect(def),

                EffectType.ChainDamage =>
                    new ChainDamageEffect(
                        def.value,
                        def.damageType,
                        def.layerMask,
                        def.chainCount,
                        def.chainRadius,
                        def.chainDamageFalloff,
                        def.ownership,
                        def.impactFxId
                    ),

                _ => null
            };
        }
    }

    public sealed class PlaySoundEffect : IEffect
    {
        private readonly EffectDefinition definition;

        public PlaySoundEffect(EffectDefinition definition)
        {
            this.definition = definition;
        }

        public void Apply(EffectContext context)
        {
            if (definition.soundConfig == null)
                return;

            var position = context != null
                ? context.Origin
                : default;

            ImpactFxDispatcher.Instance?.ServerPlaySound(definition.soundConfig, position);
        }
    }

    public sealed class ChainDamageEffect : IEffect
    {
        private static readonly Collider[] Hits = new Collider[64];

        private readonly float damage;
        private readonly DamageType damageType;
        private readonly LayerMask layerMask;
        private readonly int chainCount;
        private readonly float chainRadius;
        private readonly float damageFalloff;
        private readonly OwnershipFilter ownership;
        private readonly string impactFxId;

        public ChainDamageEffect(
            float damage,
            DamageType damageType,
            LayerMask layerMask,
            int chainCount,
            float chainRadius,
            float damageFalloff,
            OwnershipFilter ownership,
            string impactFxId)
        {
            this.damage = damage;
            this.damageType = damageType;
            this.layerMask = layerMask;
            this.chainCount = chainCount;
            this.chainRadius = chainRadius;
            this.damageFalloff = damageFalloff <= 0f ? 1f : damageFalloff;
            this.ownership = ownership;
            this.impactFxId = impactFxId;
        }

        public void Apply(EffectContext context)
        {
            if (context?.Targets == null || context.Targets.Length == 0)
                return;

            var current = context.Targets[0];
            if (current == null || !current.IsReady)
                return;

            int maxHits = chainCount <= 0 ? 1 : chainCount;
            float currentDamage = damage;
            var visited = new HashSet<IBuffTarget>();
            SpawnChain(context.Origin, current);

            for (int i = 0; i < maxHits && current != null; i++)
            {
                visited.Add(current);
                ApplyDamage(context, current, currentDamage);
                SpawnImpact(current);

                currentDamage *= damageFalloff;
                var next = FindNextTarget(context.Source, current, visited);
                if (next != null)
                    SpawnChain(current.Transform.position, next);

                current = next;
            }
        }

        private void ApplyDamage(EffectContext baseContext, IBuffTarget target, float value)
        {
            var damageContext = new EffectContext(
                baseContext.Source,
                new[] { target },
                target.Transform.position,
                Vector3.zero
            );

            EffectExecutor.Instance.Execute(
                new EffectDefinition
                {
                    type = EffectType.DealDamage,
                    targetMode = TargetMode.Explicit,
                    value = value,
                    damageType = damageType
                },
                damageContext
            );
        }

        private IBuffTarget FindNextTarget(
            IBuffSource source,
            IBuffTarget from,
            HashSet<IBuffTarget> visited)
        {
            if (from?.Transform == null || chainRadius <= 0f)
                return null;

            int hitCount = Physics.OverlapSphereNonAlloc(
                from.Transform.position,
                chainRadius,
                Hits,
                layerMask
            );

            IBuffTarget best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                var collider = Hits[i];
                if (collider == null)
                    continue;

                var target = collider.GetComponentInParent<IBuffTarget>();

                if (target == null || visited.Contains(target) || !target.IsReady)
                    continue;

                if (!PassesOwnership(source, target))
                    continue;

                float distance = (target.Transform.position - from.Transform.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = target;
                }
            }

            return best;
        }

        private bool PassesOwnership(IBuffSource source, IBuffTarget target)
        {
            if (ownership == OwnershipFilter.Any)
                return true;

            if (source == null || target.OwnerSource == null)
                return false;

            return ownership switch
            {
                OwnershipFilter.SameOwner => target.OwnerSource == source,
                OwnershipFilter.DifferentOwner => target.OwnerSource != source,
                _ => true
            };
        }

        private void SpawnImpact(IBuffTarget target)
        {
            if (string.IsNullOrEmpty(impactFxId) || target?.Transform == null)
                return;

            ImpactFxDispatcher.Instance?.ServerSpawn(
                target.Transform.position,
                Vector3.up,
                impactFxId
            );
        }

        private void SpawnChain(Vector3 start, IBuffTarget target)
        {
            if (target?.Transform == null)
                return;

            if ((start - target.Transform.position).sqrMagnitude < 0.01f)
                return;

            ImpactFxDispatcher.Instance?.ServerSpawnChain(
                start,
                target.Transform.position,
                0.12f
            );
        }
    }
}
