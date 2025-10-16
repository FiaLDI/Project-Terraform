using UnityEngine;

namespace Quests
{
    /// <summary>
    /// Базовый класс для всех типов поведения квеста.
    /// От него наследуются StandOnPointQuestBehaviour, ApproachPointQuestBehaviour и т.п.
    /// </summary>
    [System.Serializable]
    public abstract class QuestBehaviour
    {
        public virtual void StartQuest(QuestAsset quest) { }
        public virtual void UpdateProgress(QuestAsset quest, int amount = 1) { }
        public virtual void CompleteQuest(QuestAsset quest) { }
        public virtual void ResetQuest(QuestAsset quest) { }

        // Можно добавить универсальные флаги для UI
        public virtual bool IsActive => false;
        public virtual bool IsCompleted => false;
        public virtual int CurrentProgress => 0;
        public virtual int TargetProgress => 0;
        public abstract QuestBehaviour Clone();
    }
}
