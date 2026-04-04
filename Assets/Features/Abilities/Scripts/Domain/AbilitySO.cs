using UnityEngine;
using Features.Effects.Domain;

namespace Features.Abilities.Domain
{
    [CreateAssetMenu(
        fileName = "Ability",
        menuName = "Game/Abilities/Ability")]
    public sealed class AbilitySO : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        [SerializeField] private AbilityTag tags;
        public AbilityTag Tags => tags;

        [Header("UI")]
        public Sprite icon;

        [TextArea(2, 4)]
        public string description;

        [Header("Costs & Cooldowns")]
        public float energyCost = 20f;
        public float cooldown = 12f;

        [Header("Cast")]
        public AbilityCastType castType = AbilityCastType.Instant;
        public float castTime = 0f;

        [Header("Effects")]
        public EffectDefinition[] effects;
    }
}
