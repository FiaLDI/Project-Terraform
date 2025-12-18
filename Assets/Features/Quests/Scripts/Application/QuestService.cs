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
            Debug.Log("▶ StartQuest CALLED for: " + def.Id.Value);

            // УЖЕ АКТИВЕН?
            if (_active.TryGetValue(def.Id, out var existing))
            {
                Debug.Log("⚠ Quest already active: " + def.Id.Value);

                // UI должно получить состояние, если подключилось позже
                OnQuestUpdated?.Invoke(existing);
                return existing;
            }

            // СОЗДАЕМ РАНТАЙМ
            var runtime = new QuestRuntime(def);
            runtime.OnUpdated += HandleQuestUpdated;

            _active.Add(def.Id, runtime);

            // ВЫЗОВ domain-логики
            def.Behaviour.OnStart(runtime);

            Debug.Log("✔ Quest actually started: " + def.Id.Value);

            // UI: новый квест
            OnQuestAdded?.Invoke(runtime);

            // UI: обновление состояния
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
                quest.Definition.Behaviour.OnEvent(quest, e);

                // завершён?
                if (quest.State == QuestState.Completed && !_completed.Contains(quest))
                {
                    _completed.Add(quest);

                    OnQuestUpdated?.Invoke(quest);
                }
            }
        }

        // ----------------------------------------------------------
        // COMPLETE QUEST
        // ----------------------------------------------------------

        public void CompleteQuest(QuestId id)
        {
            if (_active.TryGetValue(id, out var quest))
            {
                quest.SetState(QuestState.Completed);
                _completed.Add(quest);

                OnQuestUpdated?.Invoke(quest);
            }
        }

        // ----------------------------------------------------------
        // RESET QUEST
        // ----------------------------------------------------------

        public void ResetQuest(QuestId id)
        {
            if (_active.TryGetValue(id, out var quest))
            {
                quest.Definition.Behaviour.OnReset(quest);
                quest.OnUpdated -= HandleQuestUpdated;

                _active.Remove(id);

                OnQuestRemoved?.Invoke(quest);
            }
        }

        // ----------------------------------------------------------
        // INTERNAL UPDATE FORWARD
        // ----------------------------------------------------------

        private void HandleQuestUpdated(QuestRuntime quest)
        {
            OnQuestUpdated?.Invoke(quest);
        }
    }
}
