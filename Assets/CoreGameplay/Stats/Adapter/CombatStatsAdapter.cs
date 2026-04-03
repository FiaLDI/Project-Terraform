using UnityEngine;
using Features.Stats.Domain;

namespace Features.Stats.Adapter
{
    public class CombatStatsAdapter : MonoBehaviour
    {
        private ICombatStats _stats;

        // =========================
        // PROPERTIES
        // =========================

        public float DamageMultiplier => _stats != null ? _stats.DamageMultiplier : 1f;
        public float FireRate         => _stats != null ? _stats.FireRate : 0f;
        public float Spread           => _stats != null ? _stats.Spread : 0f;
        public float AimSpread        => _stats != null ? _stats.AimSpread : 0f;
        public float Range            => _stats != null ? _stats.Range : 0f;
        public float Recoil           => _stats != null ? _stats.Recoil : 0f;
        public int   MagazineSize     => _stats != null ? _stats.MagazineSize : 0;

        public float FinalDamage => _stats != null ? _stats.FinalDamage : 0f;
        public float CritChance => _stats != null ? _stats.CritChance : 0f;
        public float CritMultiplier => _stats != null ? _stats.CritMultiplier : 0f;
        public float Penetration => _stats != null ? _stats.Penetration : 0f;

        // =========================
        // INIT
        // =========================

        public void Init(ICombatStats stats)
        {
            _stats = stats;
        }

        // =========================
        // LEGACY SUPPORT
        // =========================

        public float ApplyDamageModifiers(float baseDamage)
        {
            return baseDamage * DamageMultiplier;
        }
    }
}
