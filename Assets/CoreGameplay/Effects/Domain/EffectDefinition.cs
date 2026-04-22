using UnityEngine;
using Features.Buffs.Domain;
using Features.Weapons.Domain;

namespace Features.Effects.Domain
{
    public enum EffectType
    {
        DealDamage,
        HealInstant,
        ApplyBuff,
        RemoveBuffSource,
        SpawnPrefab,
        MineNetworkResource,
        Continuous,
        StopContinuous,
        Scan,
        DealDamageHitscan,
        SpawnProjectile,
        MeleeDamage,
        HitscanDamage,
        ScanResourceEffect,
        SpawnImpact,
        PlaySound,
        ChainDamage
    }

    public enum OwnershipFilter
    {
        Any,
        SameOwner,
        DifferentOwner
    }

    public enum TargetMode
    {
        Self,
        Area,
        Directional,
        Explicit
    }

    [System.Serializable]
    public struct EffectDefinition
    {
        public EffectType type;

        [Header("Targeting")]
        public TargetMode targetMode;
        public float radius;
        public LayerMask layerMask;

        [Header("Damage / Heal")]
        public float value;
        public DamageType damageType;

        [Header("Buff")]
        public BuffSO buff;

        [Header("Remove Buff")]
        public bool onlySpecificBuff;
        public string buffId;
        public float heightOffset;

        [Header("Cone Settings")]
        public float coneAngle;
        public float coneDistance;

        [Header("Target Selection")]
        public bool selectClosest;
        public string impactFxId;
        

        [Header("Spawn")]
        public string prefabId;
        public float lifetime;
        public bool useSourcePosition;
        [Header("Ownership")]
        public OwnershipFilter ownership;
        [Header("Mining")]
        public float miningValue;

        [Header("Continuous")]
        public float tickInterval;
        public float duration;

        public ProjectileConfig projectileConfig;
        public EffectDefinition[] childEffects;

        [Header("Sound")]
        public SoundEffectConfig soundConfig;

        [Header("Chain")]
        public int chainCount;
        public float chainRadius;
        [Range(0f, 1f)]
        public float chainDamageFalloff;
        
        public EffectDefinition Build()
        {
            var copy = this;

            return copy;
        }
    }
}
