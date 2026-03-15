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
        ScanResourceEffect
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
        Directional
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
        public float coneAngle; // угол в градусах (например 90)
        public float coneDistance; // дистанция (обычно = radius)

        [Header("Target Selection")]
        public bool selectClosest;
        

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

    }
}
