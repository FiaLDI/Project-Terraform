using UnityEngine;
using Features.Effects.Domain;

namespace Features.Tools.Data
{
    [CreateAssetMenu(menuName = "Items/Configs/Scanner")]
    public sealed class ScannerConfig : ScriptableObject
    {
        [Header("Scan Effect")]
        public EffectDefinition[] effects;

        [Header("Cooldown")]
        public float cooldown = 0.6f;
    }
}
