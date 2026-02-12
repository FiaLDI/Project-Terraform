using UnityEngine;
using Features.Effects.Domain;

namespace Features.Tools.Data
{
    [CreateAssetMenu(menuName = "Items/Configs/Tool")]
    public class ToolConfig : ScriptableObject
    {
        [Header("Base Stats")]
        public float baseMiningSpeed = 1f;
        public float baseDamage = 1f;
        public float baseRange = 3f;

        [Header("Effects")]
        public EffectDefinition[] effects;
    }
}
