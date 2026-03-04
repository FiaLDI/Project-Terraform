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
        public static readonly StatKey FireRate =
            new("combat.fireRate");

        public static readonly StatKey FlatDamage =
            new("combat.damage.flat");

        public static readonly StatKey Spread =
            new("combat.spread");

        public static readonly StatKey Recoil =
            new("combat.recoil");

        public static readonly StatKey AimSpread =
            new("combat.aimSpread");

        public static readonly StatKey Range =
            new("combat.range");

        public static readonly StatKey MagazineSize =
            new("combat.magazine");
        
        public static readonly StatKey DamageMultiplier =
            new("combat.damage.mult");

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

        public static readonly StatKey Gravity =
            new("move.gravity");

        public static readonly StatKey JumpHeight =
            new("move.jump.height");

        // =========================
        // MINING
        // =========================
        public static readonly StatKey MiningPower =
            new("mining.power");
        
        public static readonly StatKey MiningSpeed =
            new("mining.speed");
        
        // =========================
        // SCANNER
        // =========================

        public static readonly StatKey ScanRange =
            new("scan.range");

        public static readonly StatKey ScanSpeed =
            new("scan.speed");

        public static readonly StatKey Cooldown =
            new("ability.cooldown");
    }
}
