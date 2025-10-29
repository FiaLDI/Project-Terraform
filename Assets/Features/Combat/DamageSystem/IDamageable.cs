// IDamageable.cs

/// <summary>
/// Определяет контракт для любого объекта, который может получать урон.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Метод для нанесения урона объекту.
    /// </summary>
    /// <param name="damageAmount">Количество наносимого урона.</param>
    void TakeDamage(float damageAmount);
}