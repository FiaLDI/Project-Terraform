using UnityEngine;
using Features.Stats.Domain;

namespace Features.Stats.Adapter
{
    public class ProtectStatsAdapter : MonoBehaviour
    {
        private IProtectStats _stats;

        // =========================
        // PROPERTIES
        // =========================

        public float GenericResistance  => _stats != null ? _stats.GenericResistance  : 0f;
        public float ExplosionResistance => _stats != null ? _stats.ExplosionResistance : 0f;
        public float EnergyResistance    => _stats != null ? _stats.EnergyResistance    : 0f;
        public float MiningResistance    => _stats != null ? _stats.MiningResistance    : 0f;
        public float MeleeResistance     => _stats != null ? _stats.MeleeResistance     : 0f;
        public float FireResistance      => _stats != null ? _stats.FireResistance      : 0f;
        public float ElectricResistance  => _stats != null ? _stats.ElectricResistance  : 0f;
        public float PoisonResistance    => _stats != null ? _stats.PoisonResistance    : 0f;
        public float FrostResistance     => _stats != null ? _stats.FrostResistance     : 0f;
        public float AcidResistance      => _stats != null ? _stats.AcidResistance      : 0f;

        // =========================
        // INIT
        // =========================

        public void Init(IProtectStats stats)
        {
            _stats = stats;
        }

        // =========================
        // HELPERS (опционально, но полезно)
        // =========================

        public float GetResistance(StatKey key)
        {
            if (_stats == null) return 0f;

            if (key == StatKeys.GenericResistance) return GenericResistance;
            if (key == StatKeys.ExplosionResistance) return ExplosionResistance;
            if (key == StatKeys.EnergyResistance) return EnergyResistance;
            if (key == StatKeys.MiningResistance) return MiningResistance;
            if (key == StatKeys.MeleeResistance) return MeleeResistance;
            if (key == StatKeys.FireResistance) return FireResistance;
            if (key == StatKeys.ElectricResistance) return ElectricResistance;
            if (key == StatKeys.PoisonResistance) return PoisonResistance;
            if (key == StatKeys.FrostResistance) return FrostResistance;
            if (key == StatKeys.AcidResistance) return AcidResistance;

            return 0f;
        }
    }
}
