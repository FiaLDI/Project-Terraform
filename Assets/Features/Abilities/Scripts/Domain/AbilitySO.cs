using Features.Buffs.Domain;
using UnityEngine;

namespace Features.Abilities.Domain
{
    public abstract class AbilitySO : ScriptableObject, IBuffSource
    {
        [Header("Identity")]
        public string id;
        public string displayName;

        [Header("UI")]
        public Sprite icon;
        public Sprite buffIcon;

        [Header("Description")]
        [TextArea(3, 5)]
        public string description;

        [Header("Costs & Cooldowns")]
        public float energyCost = 20f;
        public float cooldown = 12f;

        [Header("Cast Settings")]
        public AbilityTarget targetType = AbilityTarget.Self;
        public AbilityCastType castType = AbilityCastType.Instant;

        [Tooltip("Время каста для Channel-способностей")]
        public float castTime = 0f;
    }
}
