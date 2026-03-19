using System;
using System.Collections.Generic;

namespace Features.Quests.Domain
{
    public sealed class QuestRuntime
    {
        public QuestDefinition Definition { get; }

        public QuestState State { get; private set; }
        public object Context { get; }

        private readonly Dictionary<IQuestCondition, int> progress = new();

        private readonly Dictionary<IQuestCondition, int> targets = new();

        public event Action<QuestRuntime> OnUpdated;

        public QuestRuntime(QuestDefinition definition, object context)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Context = context;

            State = QuestState.Active;
        }

        // ==========================================================
        // PROGRESS PER CONDITION
        // ==========================================================

        public void SetTarget(IQuestCondition condition, int value)
        {
            targets[condition] = Math.Max(0, value);
            progress.TryAdd(condition, 0);

            EvaluateCompletion();
            NotifyUpdated();
        }

        public void AddProgress(IQuestCondition condition, int delta)
        {
            if (delta == 0)
                return;

            if (!progress.ContainsKey(condition))
                progress[condition] = 0;

            progress[condition] = Math.Max(0, progress[condition] + delta);

            EvaluateCompletion();
            NotifyUpdated();
        }

        public int GetProgress(IQuestCondition condition)
        {
            return progress.TryGetValue(condition, out var value)
                ? value
                : 0;
        }

        public int GetTarget(IQuestCondition condition)
        {
            return targets.TryGetValue(condition, out var value)
                ? value
                : 0;
        }

        // ==========================================================
        // STATE
        // ==========================================================

        public void SetState(QuestState state)
        {
            if (State == state)
                return;

            State = state;
            NotifyUpdated();
        }

        private void EvaluateCompletion()
        {
            if (State != QuestState.Active)
                return;

            foreach (var cond in Definition.Conditions)
            {
                var current = GetProgress(cond);
                var target = GetTarget(cond);

                if (target <= 0)
                    return;

                if (current < target)
                    return;
            }

            State = QuestState.Completed;
        }

        public void Reset()
        {
            progress.Clear();
            targets.Clear();

            State = QuestState.Inactive;

            NotifyUpdated();
        }

        // ==========================================================
        // INTERNAL
        // ==========================================================

        private void NotifyUpdated()
        {
            OnUpdated?.Invoke(this);
        }

        public int GetTotalProgress()
        {
            int total = 0;

            foreach (var cond in Definition.Conditions)
                total += GetProgress(cond);

            return total;
        }

        public int GetTotalTarget()
        {
            int total = 0;

            foreach (var cond in Definition.Conditions)
                total += GetTarget(cond);

            return total;
        }

        public void SetProgress(IQuestCondition condition, int value)
        {
            progress[condition] = Math.Max(0, value);

            EvaluateCompletion();
            NotifyUpdated();
        }
    }
}
