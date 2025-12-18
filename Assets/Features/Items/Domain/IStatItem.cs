public interface IStatItem
{
    /// <summary>
    /// Called by EquipmentManager when an item is equipped or upgraded.
    /// Applies final runtime stats (base + upgrades + global buffs).
    /// </summary>
    void ApplyRuntimeStats(ItemRuntimeStats stats);
}
