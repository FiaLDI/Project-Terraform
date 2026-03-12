namespace Features.Quests.Domain
{
    public sealed class PointLeftEvent : IQuestEvent
    {
        public string PointId { get; }

        public PointLeftEvent(string pointId)
        {
            PointId = pointId;
        }
    }
}
