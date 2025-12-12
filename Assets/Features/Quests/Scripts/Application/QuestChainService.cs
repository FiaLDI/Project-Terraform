using System.Collections.Generic;

namespace Features.Quests.Domain
{
    public sealed class QuestChainService : IQuestChainService
    {
        private readonly IQuestService _questService;

        private readonly Dictionary<QuestId, QuestChainState> _activeChains = new();

        public QuestChainService(IQuestService questService)
        {
            _questService = questService;
        }

        public void StartChain(QuestChainDefinition chain)
        {
            if (chain.Quests.Count == 0)
                return;

            _activeChains[chain.Id] = new QuestChainState(chain, 0);
            _questService.StartQuest(chain.Quests[0]);
        }

        public void Advance(QuestId completedQuestId)
        {
            foreach (var entry in _activeChains)
            {
                var state = entry.Value;

                if (!state.TryAdvance(completedQuestId, out var nextQuestDef))
                    continue;

                if (nextQuestDef != null)
                    _questService.StartQuest(nextQuestDef);
            }
        }

        public QuestDefinition? GetNextQuest(QuestChainDefinition chain, QuestId completedId)
        {
            for (int i = 0; i < chain.Quests.Count; i++)
            {
                if (chain.Quests[i].Id.Value == completedId.Value)
                {
                    if (i + 1 < chain.Quests.Count)
                        return chain.Quests[i + 1];
                    else
                        return null;
                }
            }

            return null;
        }

        private sealed class QuestChainState
        {
            public QuestChainDefinition Chain { get; }
            public int Index { get; private set; }

            public QuestChainState(QuestChainDefinition chain, int startIndex)
            {
                Chain = chain;
                Index = startIndex;
            }

            public bool TryAdvance(QuestId completed, out QuestDefinition? nextQuest)
            {
                nextQuest = null;

                if (Chain.Quests[Index].Id.Value != completed.Value)
                    return false;

                Index++;

                if (Index < Chain.Quests.Count)
                    nextQuest = Chain.Quests[Index];

                return true;
            }
        }
    }
}
