namespace Features.Combat.Domain
{
    /// <summary>
    /// Временные модификаторы урона (криты, баффы, пробитие и т.п.)
    /// </summary>
    public struct DamageModifiers
    {
        /// <summary> Общий множитель урона (1 = без изменений) </summary>
        public float multiplier;

        /// <summary> Пробитие брони (0–1 или абсолют — как у тебя в ResistanceService) </summary>
        public float armorPenetration;

        public static DamageModifiers Default => new DamageModifiers
        {
            multiplier = 1f,
            armorPenetration = 0f
        };
    }
}
