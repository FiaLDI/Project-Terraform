namespace Features.Stats.Domain
{
    /// <summary>
    /// Центральный реестр всех поддерживаемых StatKey.
    /// Используется Effects / Stats / UI.
    /// </summary>
    public static class StatKeys
    {
        // =========================
        // COMBAT
        // =========================
        public static readonly StatKey DamageMultiplier =
            new("combat.damage.mult");

        public static readonly StatKey FireRate =
            new("combat.fireRate");

        // =========================
        // HEALTH
        // =========================
        public static readonly StatKey MaxHp =
            new("health.max");

        public static readonly StatKey HpRegen =
            new("health.regen");

        public static readonly StatKey Shield =
            new("health.shield");

        // =========================
        // ENERGY
        // =========================
        public static readonly StatKey MaxEnergy =
            new("energy.max");

        public static readonly StatKey EnergyRegen =
            new("energy.regen");

        public static readonly StatKey EnergyCostMult =
            new("energy.cost.mult");

        // =========================
        // MOVEMENT
        // =========================
        public static readonly StatKey MoveSpeed =
            new("move.base");

        public static readonly StatKey WalkSpeed =
            new("move.walk");

        public static readonly StatKey SprintSpeed =
            new("move.sprint");

        public static readonly StatKey CrouchSpeed =
            new("move.crouch");

        public static readonly StatKey RotationSpeed =
            new("move.rotation");

        // =========================
        // MINING
        // =========================
        public static readonly StatKey MiningPower =
            new("mining.power");
    }
}
