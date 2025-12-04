using UnityEngine;

namespace Features.Buffs.Domain
{
    public enum BuffTargetType
    {
        Self,       // применяется только на владельца (owner)
        Any,        // на любые IBuffTarget
        Global      // глобальный модификатор
    }

    public enum BuffStat
    {
        // PLAYER
        PlayerDamage,
        PlayerMoveSpeed,
        PlayerShield,
        PlayerMaxEnergy,
        PlayerEnergyCostReduction,
        PlayerEnergyRegen,
        PlayerMiningSpeed,

        // TURRET
        TurretDamage,
        TurretFireRate,
        TurretRotationSpeed,
        TurretMaxHP,

        // UNIVERSAL
        HealPerSecond
    }

    public enum BuffModType
    {
        Add,    // +X
        Mult,   // *X
        Set     // =X
    }

    [CreateAssetMenu(menuName = "Game/Buff/Universal")]
    public class BuffSO : ScriptableObject
    {
        [Header("Info")]
        public string buffId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;
        public bool isDebuff = false;

        [Header("Behaviour")]
        public BuffStat stat;
        public BuffModType modType = BuffModType.Add;
        public BuffTargetType targetType = BuffTargetType.Any;

        /// <summary>
        /// Add: +value | Mult: xvalue | Set: =value
        /// </summary>
        public float value;

        [Header("Timing")]
        public float duration = 5f;
        public bool isStackable = false;

        public override string ToString() =>
            $"{modType} {value} → {stat} ({duration}s; type={targetType})";
    }
}
