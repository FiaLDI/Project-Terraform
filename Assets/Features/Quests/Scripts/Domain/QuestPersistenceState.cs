using System.Collections.Generic;

namespace Features.Quests.Domain
{
    public sealed class QuestStateSnapshot
    {
        public string QuestId { get; }
        public QuestConditionNetState[] Conditions { get; }
        public QuestState State { get; }

        public QuestStateSnapshot(string questId, QuestConditionNetState[] conditions, QuestState state)
        {
            QuestId = questId;
            Conditions = conditions ?? System.Array.Empty<QuestConditionNetState>();
            State = state;
        }
    }

    public sealed class QuestChainStateSnapshot
    {
        public string ChainId { get; }
        public int Index { get; }

        public QuestChainStateSnapshot(string chainId, int index)
        {
            ChainId = chainId;
            Index = index;
        }
    }

    public sealed class QuestPersistenceState
    {
        public bool Initialized { get; set; }

        public List<QuestStateSnapshot> Quests { get; } = new();
        public List<QuestChainStateSnapshot> Chains { get; } = new();
        public HashSet<string> RewardedQuestIds { get; } = new();
        public HashSet<string> AdvancedQuestIds { get; } = new();
    }
}
