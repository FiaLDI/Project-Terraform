using UnityEngine;

namespace Features.Quests.Domain
{
    public sealed class PointLeftEvent : IQuestEvent
    {
        public GameObject Source { get; }
        public string PointId { get; }

        public PointLeftEvent(GameObject source, string pointId)
        {
            Source = source;
            PointId = pointId;
        }
    }
}

