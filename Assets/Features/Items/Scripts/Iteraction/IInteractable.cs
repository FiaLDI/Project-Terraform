// IInteractable.cs

/// <summary>
/// Определяет контракт для любого объекта, с которым можно взаимодействовать.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Сообщение-подсказка, которое будет выводиться игроку (например, "Нажать F, чтобы открыть").
    /// </summary>
    string InteractionPrompt { get; }

    /// <summary>
    /// Основной метод взаимодействия. Вызывается, когда игрок нажимает кнопку действия.
    /// </summary>
    /// <returns>Возвращает true, если взаимодействие было успешным.</returns>
    bool Interact();
}