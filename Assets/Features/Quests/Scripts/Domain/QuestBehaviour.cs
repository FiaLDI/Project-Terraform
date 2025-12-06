using System;
using System.Collections.Generic;

namespace Features.Quests.Domain
{
    public interface IQuestBehaviour
    {
        void OnStart(QuestRuntime quest);
        void OnEvent(QuestRuntime quest, IQuestEvent e);
        void OnReset(QuestRuntime quest);
    }

    public interface IQuestService
    {
        IReadOnlyCollection<QuestRuntime> ActiveQuests { get; }
        IReadOnlyCollection<QuestRuntime> CompletedQuests { get; }

        QuestRuntime StartQuest(QuestDefinition def);
        void HandleEvent(IQuestEvent e);
        void CompleteQuest(QuestId id);
        void ResetQuest(QuestId id);
    }

    public interface IQuestReadModel
    {
        event Action<QuestRuntime> OnQuestAdded;
        event Action<QuestRuntime> OnQuestUpdated;
        event Action<QuestRuntime> OnQuestRemoved;
    }
}
