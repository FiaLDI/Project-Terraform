using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Features.Quests.Domain
{
    public sealed class QuestService : IQuestService, IQuestReadModel
    {
        private readonly Dictionary<QuestId, QuestRuntime> _active = new();
        private readonly HashSet<QuestRuntime> _completed = new();

        // ----------------------------------------------------------
        // COLLECTIONS
        // ----------------------------------------------------------

        public IReadOnlyCollection<QuestRuntime> ActiveQuests
            => _active.Values.ToList().AsReadOnly();

        public IReadOnlyCollection<QuestRuntime> CompletedQuests
            => _completed.ToList().AsReadOnly();

        // ----------------------------------------------------------
        // EVENTS
        // ----------------------------------------------------------

        public event Action<QuestRuntime> OnQuestAdded;
        public event Action<QuestRuntime> OnQuestUpdated;
        public event Action<QuestRuntime> OnQuestRemoved;

        // ----------------------------------------------------------
        // START QUEST
        // ----------------------------------------------------------

        public QuestRuntime StartQuest(QuestDefinition def)
        {
            if (_active.TryGetValue(def.Id, out var existing))
                return existing;

            var runtime = new QuestRuntime(def);

            runtime.OnUpdated += HandleQuestUpdated;

            _active.Add(def.Id, runtime);

            foreach (var cond in def.Conditions)
                cond.OnStart(runtime);

            OnQuestAdded?.Invoke(runtime);
            OnQuestUpdated?.Invoke(runtime);

            return runtime;
        }

        // ----------------------------------------------------------
        // PROCESS EVENT
        // ----------------------------------------------------------

        public void HandleEvent(IQuestEvent e)
        {
            foreach (var quest in _active.Values.ToList())
            {
                if (quest.State != QuestState.Active)
                    continue;

                foreach (var condition in quest.Definition.Conditions)
                {
                    condition.OnEvent(quest, e);
                }

                if (quest.Definition.Conditions.All(c => c.IsCompleted(quest)))
                {
                    CompleteInternal(quest);
                }
            }
        }

        // ----------------------------------------------------------
        // COMPLETE
        // ----------------------------------------------------------

        private void CompleteInternal(QuestRuntime quest)
        {
            if (_completed.Contains(quest))
                return;

            quest.SetState(QuestState.Completed);

            _completed.Add(quest);

            Debug.Log($"[QuestService] Quest completed: {quest.Definition.Id}");

            OnQuestUpdated?.Invoke(quest);
        }

        public void CompleteQuest(QuestId id)
        {
            if (!_active.TryGetValue(id, out var quest))
                return;

            CompleteInternal(quest);
        }

        // ----------------------------------------------------------
        // RESET
        // ----------------------------------------------------------

        public void ResetQuest(QuestId id)
        {
            if (!_active.TryGetValue(id, out var quest))
                return;

            quest.Reset();

            quest.OnUpdated -= HandleQuestUpdated;

            _active.Remove(id);

            OnQuestRemoved?.Invoke(quest);
        }

        // ----------------------------------------------------------
        // INTERNAL UPDATE
        // ----------------------------------------------------------

        private void HandleQuestUpdated(QuestRuntime quest)
        {
            OnQuestUpdated?.Invoke(quest);
        }
    }
}
