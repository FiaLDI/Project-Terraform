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
                Destroy(gameObject);
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

            var ctx = new EffectContext(
                source: baseContext.Source,
                targets: targets,
                origin: baseContext.Origin,
                direction: baseContext.Direction
            );

            IEffect effect = def.type switch
            {
                EffectType.DealDamage => BuildDamage(def, ctx),

                _ => EffectFactory.Create(def)
            };

            effect?.Apply(ctx);
        }

        private IEffect BuildDamage(EffectDefinition def, EffectContext ctx)
        {
            if (ctx.Targets == null || ctx.Targets.Length == 0)
                return null;

            var target = ctx.Targets[0];

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

            // =========================
            // SOURCE (урон)
            // =========================

            if (sourceStats?.Combat != null)
            {
                var s = sourceStats.Combat;

                value *= s.DamageMultiplier;

                if (s.CritChance > 0f && UnityEngine.Random.value <= s.CritChance)
                {
                    value *= s.CritMultiplier;
                }
            }

            // =========================
            // RESIST
            // =========================

            float resist = GetResistance(targetStats, def.damageType);

            // =========================
            // PENETRATION
            // =========================

            float penetration = sourceStats?.Combat?.Penetration ?? 0f;

            float finalResist = resist * (1f - penetration);
            
            finalResist = Mathf.Clamp01(finalResist);

            value *= (1f - finalResist);

            // =========================
            // SAFETY
            // =========================

            return Mathf.Max(0f, value);
        }

        private float GetResistance(IStatsFacade stats, DamageType type)
        {
            var p = stats?.Protect;
            if (p == null)
                return 0f;

            return type switch
            {
                DamageType.Generic   => p.GenericResistance,
                DamageType.Explosion => p.ExplosionResistance,
                DamageType.Energy    => p.EnergyResistance,
                DamageType.Mining    => p.MiningResistance,
                DamageType.Melee     => p.MeleeResistance,
                DamageType.Fire      => p.FireResistance,
                DamageType.Electric  => p.ElectricResistance,
                DamageType.Poison    => p.PoisonResistance,
                DamageType.Frost     => p.FrostResistance,
                DamageType.Acid      => p.AcidResistance,
                _ => 0f
            };
        }
    }
}
