namespace Features.Buffs.Domain
{
    /// <summary>
    /// Определяет, как живёт бафф
    /// </summary>
    public enum BuffLifetimeMode
    {
        /// <summary>
        /// Бафф живёт по duration
        /// </summary>
        Duration,

        /// <summary>
        /// Бафф живёт пока источник жив
        /// (пассив, класс, предмет, аура)
        /// </summary>
        WhileSourceAlive
    }
}
