using Features.Stats.Domain;

namespace Features.Stats.Domain
{
    public sealed class ProtectStats : IProtectStats, IStatModifierTarget
    {
        // ================= BASE =================
        private float _baseGenericResistance;
        private float _baseExplosionResistance;
        private float _baseEnergyResistance;
        private float _baseMiningResistance;
        private float _baseMeleeResistance;
        private float _baseFireResistance;
        private float _baseElectricResistance;
        private float _basePoisonResistance;
        private float _baseFrostResistance;
        private float _baseAcidResistance;

        // ================= ADD =================
        private float _addGenericResistance;
        private float _addExplosionResistance;
        private float _addEnergyResistance;
        private float _addMiningResistance;
        private float _addMeleeResistance;
        private float _addFireResistance;
        private float _addElectricResistance;
        private float _addPoisonResistance;
        private float _addFrostResistance;
        private float _addAcidResistance;

        // ================= MULT =================
        private float _multGenericResistance = 1f;
        private float _multExplosionResistance = 1f;
        private float _multEnergyResistance = 1f;
        private float _multMiningResistance = 1f;
        private float _multMeleeResistance = 1f;
        private float _multFireResistance = 1f;
        private float _multElectricResistance = 1f;
        private float _multPoisonResistance = 1f;
        private float _multFrostResistance = 1f;
        private float _multAcidResistance = 1f;

        // ================= FINAL =================
        public float GenericResistance => (_baseGenericResistance + _addGenericResistance) * _multGenericResistance;
        public float ExplosionResistance => (_baseExplosionResistance + _addExplosionResistance) * _multExplosionResistance;
        public float EnergyResistance => (_baseEnergyResistance + _addEnergyResistance) * _multEnergyResistance;
        public float MiningResistance => (_baseMiningResistance + _addMiningResistance) * _multMiningResistance;
        public float MeleeResistance => (_baseMeleeResistance + _addMeleeResistance) * _multMeleeResistance;
        public float FireResistance => (_baseFireResistance + _addFireResistance) * _multFireResistance;
        public float ElectricResistance => (_baseElectricResistance + _addElectricResistance) * _multElectricResistance;
        public float PoisonResistance => (_basePoisonResistance + _addPoisonResistance) * _multPoisonResistance;
        public float FrostResistance => (_baseFrostResistance + _addFrostResistance) * _multFrostResistance;
        public float AcidResistance => (_baseAcidResistance + _addAcidResistance) * _multAcidResistance;

        // ================= BASE APPLY =================
        public void ApplyBase(
            float genericResistance,
            float explosionResistance,
            float energyResistance,
            float miningResistance,
            float meleeResistance,
            float fireResistance,
            float electricResistance,
            float poisonResistance,
            float frostResistance,
            float acidResistance)
        {
            _baseGenericResistance = genericResistance;
            _baseExplosionResistance = explosionResistance;
            _baseEnergyResistance = energyResistance;
            _baseMiningResistance = miningResistance;
            _baseMeleeResistance = meleeResistance;
            _baseFireResistance = fireResistance;
            _baseElectricResistance = electricResistance;
            _basePoisonResistance = poisonResistance;
            _baseFrostResistance = frostResistance;
            _baseAcidResistance = acidResistance;
        }

        // ================= MODIFIERS =================
        public bool TryAdd(StatKey key, float value)
        {
            if (key == StatKeys.GenericResistance) { _addGenericResistance += value; return true; }
            if (key == StatKeys.ExplosionResistance) { _addExplosionResistance += value; return true; }
            if (key == StatKeys.EnergyResistance) { _addEnergyResistance += value; return true; }
            if (key == StatKeys.MiningResistance) { _addMiningResistance += value; return true; }
            if (key == StatKeys.MeleeResistance) { _addMeleeResistance += value; return true; }
            if (key == StatKeys.FireResistance) { _addFireResistance += value; return true; }
            if (key == StatKeys.ElectricResistance) { _addElectricResistance += value; return true; }
            if (key == StatKeys.PoisonResistance) { _addPoisonResistance += value; return true; }
            if (key == StatKeys.FrostResistance) { _addFrostResistance += value; return true; }
            if (key == StatKeys.AcidResistance) { _addAcidResistance += value; return true; }

            return false;
        }

        public bool TryMultiply(StatKey key, float value)
        {
            if (key == StatKeys.GenericResistance) { _multGenericResistance *= value; return true; }
            if (key == StatKeys.ExplosionResistance) { _multExplosionResistance *= value; return true; }
            if (key == StatKeys.EnergyResistance) { _multEnergyResistance *= value; return true; }
            if (key == StatKeys.MiningResistance) { _multMiningResistance *= value; return true; }
            if (key == StatKeys.MeleeResistance) { _multMeleeResistance *= value; return true; }
            if (key == StatKeys.FireResistance) { _multFireResistance *= value; return true; }
            if (key == StatKeys.ElectricResistance) { _multElectricResistance *= value; return true; }
            if (key == StatKeys.PoisonResistance) { _multPoisonResistance *= value; return true; }
            if (key == StatKeys.FrostResistance) { _multFrostResistance *= value; return true; }
            if (key == StatKeys.AcidResistance) { _multAcidResistance *= value; return true; }

            return false;
        }

        public void Reset()
        {
            _addGenericResistance = 0f;
            _addExplosionResistance = 0f;
            _addEnergyResistance = 0f;
            _addMiningResistance = 0f;
            _addMeleeResistance = 0f;
            _addFireResistance = 0f;
            _addElectricResistance = 0f;
            _addPoisonResistance = 0f;
            _addFrostResistance = 0f;
            _addAcidResistance = 0f;

            _multGenericResistance = 1f;
            _multExplosionResistance = 1f;
            _multEnergyResistance = 1f;
            _multMiningResistance = 1f;
            _multMeleeResistance = 1f;
            _multFireResistance = 1f;
            _multElectricResistance = 1f;
            _multPoisonResistance = 1f;
            _multFrostResistance = 1f;
            _multAcidResistance = 1f;
        }
    }
}
