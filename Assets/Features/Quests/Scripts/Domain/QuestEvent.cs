namespace Features.Quests.Domain
{
    public interface IQuestEvent { }

    // Уничтожен враг
    public sealed class EnemyKilledEvent : IQuestEvent
    {
        public string EnemyTag { get; }
        public EnemyKilledEvent(string enemyTag) => EnemyTag = enemyTag;
    }

    // Добавлен предмет в инвентарь
    public sealed class ItemAddedEvent : IQuestEvent
    {
        public string ItemId { get; }
        public int Amount { get; }

        public ItemAddedEvent(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }

    // Игрок достиг точки (по id точки, а не Transform)
    public sealed class PointReachedEvent : IQuestEvent
    {
        public string PointId { get; }
        public PointReachedEvent(string pointId) => PointId = pointId;
    }

    // Игрок взаимодействовал с точкой
    public sealed class InteractionEvent : IQuestEvent
    {
        public string PointId { get; }
        public InteractionEvent(string pointId) => PointId = pointId;
    }

    // Таймер (для “постоять на точке N секунд” и т.п.)
    public sealed class TickEvent : IQuestEvent
    {
        public float DeltaTime { get; }
        public TickEvent(float deltaTime) => DeltaTime = deltaTime;
    }
}
