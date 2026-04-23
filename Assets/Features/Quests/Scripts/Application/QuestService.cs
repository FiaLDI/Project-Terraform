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

        private readonly GameObject owner;

        public QuestService(GameObject owner)
        {
            this.owner = owner;
        }

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

            var runtime = new QuestRuntime(def, owner);

            runtime.OnUpdated += HandleQuestUpdated;

            _active.Add(def.Id, runtime);

            foreach (var cond in def.Conditions)
                cond.OnStart(runtime);

            OnQuestAdded?.Invoke(runtime);
            OnQuestUpdated?.Invoke(runtime);

            return runtime;
        }

        public QuestRuntime RestoreQuest(
            QuestDefinition def,
            QuestConditionNetState[] conditions,
            QuestState state)
        {
            if (_active.TryGetValue(def.Id, out var existing))
                return existing;

            var runtime = new QuestRuntime(def, owner);
            _active.Add(def.Id, runtime);

            foreach (var cond in def.Conditions)
                cond.OnStart(runtime);

            if (conditions != null)
            {
                int count = Math.Min(def.Conditions.Count, conditions.Length);
                for (int i = 0; i < count; i++)
                {
                    var net = conditions[i];
                    runtime.RestoreConditionState(def.Conditions[i], net.progress, net.target);
                }
            }

            runtime.RestoreState(state);
            runtime.OnUpdated += HandleQuestUpdated;

            if (state == QuestState.Completed)
                _completed.Add(runtime);

            return runtime;
        }

        // ----------------------------------------------------------
        // PROCESS EVENT
        // ----------------------------------------------------------

        public void HandleEventFiltered(IQuestEvent e, bool isOwnerEvent)
        {
            foreach (var quest in _active.Values.ToList())
            {
                if (quest.State != QuestState.Active)
                    continue;

                if (quest.Definition.Scope == QuestScope.Personal && !isOwnerEvent)
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

        public void HandleEvent(IQuestEvent e)
        {
            foreach (var quest in _active.Values.ToList())
            {
                if (quest.State != QuestState.Active)
                    continue;

                // ❗ ВАЖНО
                if (quest.Definition.Scope == QuestScope.Personal)
                {
                    // только если событие от владельца
                    if (e.Source != owner)
                        continue;
                }

                // Shared — пропускаем ВСЕ события

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
            _completed.Remove(quest);

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

        public bool TryGetQuest(QuestId id, out QuestRuntime quest)
        {
            return _active.TryGetValue(id, out quest);
        }

        public void FailQuest(QuestId id)
        {
            if (!_active.TryGetValue(id, out var quest))
                return;

            _completed.Remove(quest);
            quest.SetState(QuestState.Failed);

            Debug.Log($"[QuestService] Quest failed: {id}");

            OnQuestUpdated?.Invoke(quest);
        }
    }
}
