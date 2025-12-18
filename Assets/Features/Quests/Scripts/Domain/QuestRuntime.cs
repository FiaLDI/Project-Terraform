using System;

namespace Features.Quests.Domain
{
    public sealed class QuestRuntime
    {
        public QuestDefinition Definition { get; }

        public QuestState State { get; private set; } = QuestState.Inactive;

        public int CurrentProgress { get; private set; }
        public int TargetProgress { get; private set; }

        /// <summary>
        /// Вызывается всегда, когда что-то меняется: прогресс, состояние, таргет, поведение.
        /// </summary>
        public event Action<QuestRuntime> OnUpdated;

        public QuestRuntime(QuestDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            State = QuestState.Active;
        }


        // ==========================================================
        // PROGRESS CONTROL
        // ==========================================================

        public void SetTarget(int target)
        {
            TargetProgress = Math.Max(0, target);
            EvaluateCompletion();
            NotifyUpdated();
        }

        public void SetProgress(int value)
        {
            CurrentProgress = Math.Max(0, value);
            EvaluateCompletion();
            NotifyUpdated();
        }

        public void AddProgress(int delta)
        {
            CurrentProgress = Math.Max(0, CurrentProgress + delta);
            EvaluateCompletion();
            NotifyUpdated();
        }


        // ==========================================================
        // STATE CONTROL
        // ==========================================================

        /// <summary>
        /// Принудительно завершить квест из Behaviour или сервиса.
        /// </summary>
        public void SetState(QuestState state)
        {
            State = state;
            NotifyUpdated();
        }

        /// <summary>
        /// Логически завершает квест, если достижён таргет.
        /// </summary>
        private void EvaluateCompletion()
        {
            if (State != QuestState.Completed &&
                TargetProgress > 0 &&
                CurrentProgress >= TargetProgress)
            {
                State = QuestState.Completed;
            }
        }

        /// <summary>
        /// Сброс прогресса для переиспользования квеста.
        /// </summary>
        public void Reset()
        {
            CurrentProgress = 0;
            TargetProgress = 0;
            State = QuestState.Inactive;
            NotifyUpdated();
        }


        // ==========================================================
        // INTERNAL HELPERS
        // ==========================================================

        private void NotifyUpdated()
        {
            OnUpdated?.Invoke(this);
        }
    }
}
