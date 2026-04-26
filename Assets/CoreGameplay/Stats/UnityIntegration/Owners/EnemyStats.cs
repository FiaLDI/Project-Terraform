using Features.Buffs.Domain;
using Features.Enemy.Data;
using Features.Items.UnityIntegration;
using Features.Player.UnityIntegration;
using Features.Quests.Application;
using Features.Quests.Domain;
using Features.Stats.Domain;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace Features.Stats.UnityIntegration
{
    [DefaultExecutionOrder(-400)]
    [RequireComponent(typeof(StatsBuffTarget))]
    public sealed class EnemyStats : StatsOwnerBase
    {
        [Header("Config")]
        [SerializeField] private EnemyConfigSO config;
        private IBuffSource lastAttacker;
        private bool isDead;
        private bool runtimeScaleConfigured;
        private bool runtimeScaleApplied;
        private float runtimeStatScale = 1f;

        // =========================
        // SERVER
        // =========================

        protected override void InitStats()
        {
            base.InitStats();

            if (config == null)
            {
                Debug.LogError("[EnemyStats] EnemyConfigSO not assigned", this);
                return;
            }

            ApplyDefaultsFromConfig();
            ApplyRuntimeScalingIfReady();
        }

        private void ApplyDefaultsFromConfig()
        {
            if (config.stats != null)
            {
                ApplyPreset(config.stats);
                return;
            }

            ApplyFallbackDefaults();
        }

        private void ApplyFallbackDefaults()
        {
            if (Facade.Health != null)
            {
                Facade.Health.ApplyBase(100f);
                Facade.Health.ApplyRegenBase(0f);
            }

            if (Facade.Combat != null)
            {
                Facade.Combat.ApplyBase(
                    baseDamage: 1f,
                    fireRate: 6f,
                    spread: 2f,
                    aimSpread: 0.5f,
                    range: 100f,
                    recoil: 1f,
                    magazineSize: 30,
                    critChance: 0.2f,
                    critMultiplier: 2f,
                    penetration: 0f
                );
            }
        }

        private void ApplyPreset(EnemyStatsPresetSO preset)
        {
            if (preset == null)
                return;

            if (Facade.Health != null)
            {
                Facade.Health.ApplyBase(preset.health.baseHp);
                Facade.Health.ApplyRegenBase(preset.health.baseRegen);
            }

            if (Facade.Combat != null)
            {
                Facade.Combat.ApplyBase(
                    baseDamage: preset.combat.baseDamageMultiplier,
                    fireRate: preset.combat.baseFireRate,
                    spread: preset.combat.baseSpread,
                    aimSpread: preset.combat.baseAimSpread,
                    range: preset.combat.baseRange,
                    recoil: preset.combat.baseRecoil,
                    magazineSize: preset.combat.baseMagazineSize,
                    critChance: 0.2f,
                    critMultiplier: 2f,
                    penetration: 0f
                );
            }
        }

        [Server]
        public void ConfigureRuntimeScaling(float scale)
        {
            runtimeScaleConfigured = true;
            runtimeStatScale = Mathf.Max(0.1f, scale);
            ApplyRuntimeScalingIfReady();
        }

        private void ApplyRuntimeScalingIfReady()
        {
            if (!runtimeScaleConfigured || runtimeScaleApplied || Facade == null)
                return;

            Facade.TryMultiply(StatKeys.MaxHp, runtimeStatScale);
            Facade.TryMultiply(StatKeys.DamageMultiplier, runtimeStatScale);
            runtimeScaleApplied = true;
        }

        public void RegisterAttacker(IBuffSource attacker)
        {
            lastAttacker = attacker;
        }

        private void Update()
        {
            if (!IsServer)
                return;

            CheckDeath();
        }

        private void CheckDeath()
        {
            if (isDead)
                return;

            if (Facade.Health == null)
                return;

            if (Facade.Health.CurrentHp > 0)
                return;

            isDead = true;

            Debug.Log("[Enemy] Died");

            var attackerGO = ResolveQuestSource(lastAttacker);

            if (attackerGO == null)
            {
                Debug.LogWarning("[EnemyStats] Attacker has no GameObject");
                return;
            }

            QuestEventBus.Publish(
                new EnemyKilledEvent(
                    attackerGO,
                    config.enemyId,
                    lastAttacker
                )
            );

            if (InstanceFinder.IsServer)
            {
                var netObj = GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    InstanceFinder.ServerManager.Despawn(netObj);
                }
                else
                {
                    Destroy(gameObject); // fallback
                }
            }
        }

        private static GameObject ResolveQuestSource(IBuffSource source, int depth = 0)
        {
            if (source == null || depth > 8)
                return null;

            if (source is StatsBuffTarget target)
            {
                var owner = target.OwnerSource;
                if (owner != null && !ReferenceEquals(owner, source))
                {
                    var resolvedOwner = ResolveQuestSource(owner, depth + 1);
                    if (resolvedOwner != null)
                        return resolvedOwner;
                }

                return ResolvePlayerRoot(target.gameObject);
            }

            if (source is ItemRuntimeSource itemSource)
            {
                var owner = itemSource.OwnerSource;
                if (owner != null && !ReferenceEquals(owner, source))
                {
                    var resolvedOwner = ResolveQuestSource(owner, depth + 1);
                    if (resolvedOwner != null)
                        return resolvedOwner;
                }

                return ResolvePlayerRoot(itemSource.gameObject);
            }

            if (source is RuntimeBuffSource runtimeSource)
            {
                if (runtimeSource.Owner is IBuffSource ownerSource)
                {
                    var resolvedOwner = ResolveQuestSource(ownerSource, depth + 1);
                    if (resolvedOwner != null)
                        return resolvedOwner;
                }

                if (runtimeSource.Owner is Component ownerComponent)
                    return ResolvePlayerRoot(ownerComponent.gameObject);
            }

            if (source is Component component)
                return ResolvePlayerRoot(component.gameObject);

            return null;
        }

        private static GameObject ResolvePlayerRoot(GameObject source)
        {
            if (source == null)
                return null;

            var player = source.GetComponentInParent<NetworkPlayer>();
            return player != null ? player.gameObject : source;
        }
    }
}
