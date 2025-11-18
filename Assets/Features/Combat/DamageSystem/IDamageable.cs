/// <summary>
/// Базовый интерфейс для всего, что получает урон и может быть исцелено.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Получить урон.
    /// </summary>
    /// <param name="damageAmount">Кол-во урона.</param>
    /// <param name="damageType">Тип урона.</param>
    void TakeDamage(float damageAmount, DamageType damageType);

    /// <summary>
    /// Получить лечение (восстановление HP).
    /// </summary>
    /// <param name="healAmount">Кол-во лечения.</param>
    void Heal(float healAmount);
}
