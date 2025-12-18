using System;
using System.Collections.Generic;

namespace Features.Quests.Domain.Behaviours
{
    public sealed class CollectItemsQuestBehaviour : IQuestBehaviour
    {
        public sealed class Requirement
        {
            public string ItemId { get; }
            public int Required { get; }

            public int Collected { get; set; }

            public Requirement(string itemId, int required)
            {
                ItemId = itemId;
                Required = required;
            }
        }

        private readonly List<Requirement> _requirements;

        public CollectItemsQuestBehaviour(IEnumerable<Requirement> requirements)
        {
            _requirements = new List<Requirement>(requirements);
        }

        public void OnStart(QuestRuntime quest)
        {
            foreach (var r in _requirements)
                r.Collected = 0;

            RecalculateProgress(quest);
            quest.SetState(QuestState.Active);
        }

        public void OnEvent(QuestRuntime quest, IQuestEvent e)
        {
            if (quest.State != QuestState.Active)
                return;

            if (e is not ItemAddedEvent ia)
                return;

            foreach (var r in _requirements)
            {
                if (r.ItemId != ia.ItemId)
                    continue;

                int need = r.Required - r.Collected;
                if (need <= 0)
                    continue;

                int credit = Math.Min(need, ia.Amount);
                r.Collected += credit;
            }

            RecalculateProgress(quest);
        }

        private void RecalculateProgress(QuestRuntime quest)
        {
            int current = 0;
            int target = 0;

            foreach (var r in _requirements)
            {
                current += r.Collected;
                target += r.Required;
            }

            quest.SetTarget(target);
            quest.SetProgress(current);

            if (target > 0 && current >= target)
            {
                quest.SetState(QuestState.Completed);
            }
        }

        public void OnReset(QuestRuntime quest)
        {
            foreach (var r in _requirements)
                r.Collected = 0;

            quest.SetProgress(0);
            quest.SetTarget(0);
            quest.SetState(QuestState.Inactive);
        }
    }
}
