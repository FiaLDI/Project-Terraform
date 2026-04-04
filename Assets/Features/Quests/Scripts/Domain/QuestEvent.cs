using UnityEngine;

namespace Features.Quests.Domain
{
    public interface IQuestEvent
    {
        GameObject Source { get; }
    }

    public sealed class PointReachedEvent : IQuestEvent
    {
        public GameObject Source { get; }
        public string PointId { get; }

        public PointReachedEvent(GameObject source, string pointId)
        {
            Source = source;
            PointId = pointId;
        }
    }

    // Игрок взаимодействовал с точкой
    public sealed class InteractionEvent : IQuestEvent
    {
        public GameObject Source { get; }
        public string PointId { get; }

        public InteractionEvent(GameObject source, string pointId)
        {
            Source = source;
            PointId = pointId;
        }
    }
    

    // Таймер (для “постоять на точке N секунд” и т.п.)
     public sealed class TickEvent : IQuestEvent
    {
        public GameObject Source { get; }
        public float DeltaTime { get; }

        public TickEvent(GameObject source, float deltaTime)
        {
            Source = source;
            DeltaTime = deltaTime;
        }
    }
}
