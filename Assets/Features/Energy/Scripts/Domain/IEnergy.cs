namespace Features.Energy.Domain
{
    /// <summary>
    /// Универсальный интерфейс энергии для способностей и систем расхода.
    /// Работает поверх EnergyStatsAdapter (Stats v2).
    /// </summary>
    public interface IEnergy
    {
        float CurrentEnergy { get; }
        float MaxEnergy { get; }
        float Regen { get; }

        /// <summary>
        /// Проверить хватает ли энергии.
        /// </summary>
        bool HasEnergy(float cost);

        /// <summary>
        /// Потратить энергию. Возвращает false, если недостаточно.
        /// </summary>
        bool TrySpend(float cost);

        /// <summary>
        /// UI callback.
        /// </summary>
        event System.Action<float, float> OnEnergyChanged;
    }
}
