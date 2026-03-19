namespace Features.Quests.Domain
{
    public interface IQuestEvent { }

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
