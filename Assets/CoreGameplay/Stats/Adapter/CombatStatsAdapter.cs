using UnityEngine;
using Features.Stats.Domain;

namespace Features.Stats.Adapter
{
    public class CombatStatsAdapter : MonoBehaviour
    {
        private ICombatStats _stats;

        public float DamageMultiplier => _stats.DamageMultiplier;

        public void Init(ICombatStats stats)
        {
            _stats = stats;
        }

        // пример API для старых систем
        public float ApplyDamageModifiers(float baseDamage)
        {
            return baseDamage * _stats.DamageMultiplier;
        }
    }
}
