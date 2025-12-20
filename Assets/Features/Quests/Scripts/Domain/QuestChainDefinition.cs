using System.Collections.Generic;

#nullable enable
namespace Features.Quests.Domain
{
    public sealed class QuestChainDefinition
    {
        public QuestId Id { get; }
        public IReadOnlyList<QuestDefinition> Quests { get; }

        public QuestChainDefinition(QuestId id, IReadOnlyList<QuestDefinition> quests)
        {
            Id = id;
            Quests = quests;
        }
    }

    public interface IQuestChainService
    {
        void StartChain(QuestChainDefinition chain);
        void Advance(QuestId questId);
        QuestDefinition? GetNextQuest(QuestChainDefinition chain, QuestId completedId);
    }
}
#nullable restore