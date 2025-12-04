namespace Features.Energy.Domain
{
    /// <summary>
    /// Модификатор стоимости энергии для abilities.
    /// Например: стойки, баффы, эффекты класса, экипировка.
    /// </summary>
    public interface IEnergyCostModifier
    {
        /// <summary>
        /// Возвращает модификатор стоимости (может быть отрицательным).
        /// 1.0 = без изменений
        /// 1.2 = +20% стоимость
        /// 0.8 = -20% стоимость
        /// </summary>
        float ModifyCost(float baseCost);
    }
}
