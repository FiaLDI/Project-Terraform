using UnityEngine;

namespace Quests
{
    [CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/Quest Asset")]
    public class QuestAsset : ScriptableObject
    {
        [Header("Основные данные")]
        public string questID;
        public string questName;
        [TextArea] public string description;

        [Header("Прогресс выполнения (автоматически)")]
        [HideInInspector] public int currentProgress;
        [HideInInspector] public int targetProgress;

        [Header("Тип поведения квеста")]
        [SerializeReference] 
        public QuestBehaviour behaviour;

        public delegate void QuestUpdated(QuestAsset quest);
        public event QuestUpdated OnQuestUpdated;

        /// <summary>Сброс прогресса</summary>
        public void ResetProgress()
        {
            currentProgress = 0;
            targetProgress = 0;
            NotifyQuestUpdated();
        }

        /// <summary>Регистрируем новую цель</summary>
        public void RegisterTarget()
        {
            targetProgress++;
            NotifyQuestUpdated();
        }

        /// <summary>Одна цель выполнена</summary>
        public void TargetCompleted()
        {
            currentProgress++;
            NotifyQuestUpdated();
        }

        /// <summary>Квест завершён?</summary>
        public bool IsCompleted => currentProgress >= targetProgress && targetProgress > 0;

        public void NotifyQuestUpdated()
        {
            OnQuestUpdated?.Invoke(this);
        }
    }
}
