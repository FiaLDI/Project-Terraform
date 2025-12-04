using Features.Buffs.Domain;

namespace Features.Stats.Domain
{
    // =============================
    //       COMBAT
    // =============================
    public interface ICombatStatReceiver
    {
        /// <summary> +урон, мульты, криты и т.д. </summary>
        void ApplyCombatBuff(BuffSO config, bool apply);
    }

    // =============================
    //       MOVEMENT
    // =============================
    public interface IMovementStatReceiver
    {
        /// <summary> бег, ходьба, присед, спец. ускорения </summary>
        void ApplyMovementBuff(BuffSO config, bool apply);
    }

    // =============================
    //       MINING
    // =============================
    public interface IMiningStatReceiver
    {
        void ApplyMiningBuff(BuffSO config, bool apply);
    }

    // =============================
    //       ENERGY
    // =============================
    public interface IEnergyStatReceiver
    {
        /// <summary> max energy + regen </summary>
        void ApplyEnergyBuff(BuffSO config, bool apply);
    }

    // =============================
    //       SHIELD
    // =============================
    public interface IShieldReceiver
    {
        void ApplyShieldBuff(BuffSO config, bool apply);
    }

    // =============================
    //       HEALTH
    // =============================
    public interface IHealthReceiver
    {
        void Heal(float amount);
        void ApplyMaxHpBuff(BuffSO config, bool apply);
    }

    // =============================
    //       TURRET
    // =============================
    public interface ITurretStatReceiver
    {
        void ApplyTurretBuff(BuffSO config, bool apply);
    }
}
