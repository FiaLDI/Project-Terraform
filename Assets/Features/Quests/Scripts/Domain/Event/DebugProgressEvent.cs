using UnityEngine;

namespace Features.Quests.Domain
{
    public struct DebugProgressEvent : IQuestEvent
    {
        public GameObject Source { get; }
        public QuestId QuestId;
        public int Amount;

        public DebugProgressEvent(GameObject source, QuestId questId, int amount)
        {
            Source = source;
            QuestId = questId;
            Amount = amount;
        }
    }
}
