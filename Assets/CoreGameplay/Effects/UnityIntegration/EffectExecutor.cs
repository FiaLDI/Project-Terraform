using System;
using System.Linq;
using Features.Buffs.Domain;
using Features.Effects.Domain;
using Features.Stats.Domain;
using FishNet;
using UnityEngine;

namespace Features.Effects.Application
{
    public sealed class EffectExecutor : MonoBehaviour
    {
        public static EffectExecutor Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Execute(EffectDefinition def, EffectContext baseContext)
        {
            if (!InstanceFinder.IsServer)
                return;

            var targets = TargetResolver.Resolve(def, baseContext);

            bool requiresTarget = def.type switch
            {
                EffectType.SpawnPrefab => false,
                EffectType.SpawnImpact => false,
                EffectType.SpawnProjectile => false,
                _ => true
            };

            if (requiresTarget && (targets == null || targets.Length == 0))
                return;

            EffectContext ctx;

            if (baseContext is HitEffectContext hit)
            {
                var hitCtx = EffectContextPool.Get<HitEffectContext>(
                    baseContext.Source,
                    targets,
                    baseContext.Origin,
                    baseContext.Direction
                );

                hitCtx.UpdateHit(hit.HitPoint, hit.HitNormal);
                ctx = hitCtx;
            }
            else
            {
                ctx = EffectContextPool.Get(
                    baseContext.Source,
                    targets,
                    baseContext.Origin,
                    baseContext.Direction
                );
            }

            try
            {
                IEffect effect = def.type switch
                {
                    EffectType.DealDamage => BuildDamage(def, ctx),
                    _ => EffectCache.Get(def)
                };

                effect?.Apply(ctx);
            }
            finally
            {
                EffectContextPool.Release(ctx);
            }
        }

        private IEffect BuildDamage(EffectDefinition def, EffectContext ctx)
        {
            var target = ctx.Targets[0];
            if (target == null)
                return null;

            float damage = CalculateDamage(def, target, ctx.Source);

            return new DealDamageEffect(damage, def.damageType);
        }

        private float CalculateDamage(
            EffectDefinition def,
            IBuffTarget target,
            IBuffSource source)
        {
            float value = def.value;

            var sourceStats = (source as IBuffTarget)?.GetServerStats();
            var targetStats = target?.GetServerStats();

            if (sourceStats?.Combat != null)
            {
                var s = sourceStats.Combat;

                value *= s.DamageMultiplier;

                if (s.CritChance > 0f && UnityEngine.Random.value <= s.CritChance)
                {
                    value *= s.CritMultiplier;
                }
            }

            float resist = GetResistance(targetStats, def.damageType);

            float penetration = sourceStats?.Combat?.Penetration ?? 0f;

            float finalResist = resist * (1f - penetration);
            finalResist = Mathf.Clamp01(finalResist);

            value *= (1f - finalResist);

            return Mathf.Max(0f, value);
        }

        private float GetResistance(IStatsFacade stats, DamageType type)
        {
            var p = stats?.Protect;
            if (p == null)
                return 0f;

            return type switch
            {
                DamageType.Generic => p.GenericResistance,
                DamageType.Explosion => p.ExplosionResistance,
                DamageType.Energy => p.EnergyResistance,
                DamageType.Mining => p.MiningResistance,
                DamageType.Melee => p.MeleeResistance,
                DamageType.Fire => p.FireResistance,
                DamageType.Electric => p.ElectricResistance,
                DamageType.Poison => p.PoisonResistance,
                DamageType.Frost => p.FrostResistance,
                DamageType.Acid => p.AcidResistance,
                _ => 0f
            };
        }
    }
}
