namespace Features.Energy.Domain
{
    /// <summary>
    /// Простая структура стоимости энергии.
    /// Используется в AbilityService → EnergyCostService.
    /// </summary>
    public struct EnergyCost
    {
        public float value;

        public EnergyCost(float v)
        {
            value = v;
        }
    }
}
