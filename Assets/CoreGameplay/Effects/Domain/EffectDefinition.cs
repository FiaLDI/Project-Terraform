using UnityEngine;
using Features.Buffs.Domain;

namespace Features.Effects.Domain
{
    public enum EffectType
    {
        DealDamage,
        HealInstant,
        ApplyBuff,
        RemoveBuffSource,
        SpawnPrefab
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

        [Header("Deal / Heal")]
        public float value;

        [Header("Buff")]
        public BuffSO buff;

        [Header("Remove Buff")]
        public bool onlySpecificBuff;
        public string buffId;

        [Header("Spawn")]
        public string prefabId;
        public float lifetime;
        public bool useSourcePosition;
    }
}
