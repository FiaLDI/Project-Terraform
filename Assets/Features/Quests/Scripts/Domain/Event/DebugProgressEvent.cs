namespace Features.Quests.Domain
{
    public struct DebugProgressEvent : IQuestEvent
    {
        public QuestId QuestId;
        public int Amount;

        public DebugProgressEvent(QuestId questId, int amount)
        {
            QuestId = questId;
            Amount = amount;
        }
    }
}
