using UnityEngine;

namespace Quests
{
    [CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/Base Quest")]
    public class Quest : ScriptableObject
    {
        [Header("Базовая информация")]
        public string questID;
        public string questName;
        [TextArea] public string description;
        public bool isCompleted;
        public bool isActive;

        [Header("Прогресс квеста")]
        public int currentProgress;
        public int targetProgress = 1;

        [Header("Связанные объекты")]
        public GameObject questMarkerPrefab;

        public delegate void QuestUpdated(Quest quest);
        public event QuestUpdated OnQuestUpdated;
        protected void NotifyQuestUpdated()
        {
            OnQuestUpdated?.Invoke(this);
        }

        public virtual void StartQuest()
        {
            if (targetProgress <= 0)
                targetProgress = 1;

            currentProgress = 0;
            isCompleted = false;
            isActive = true;

            Debug.Log($"[Quest] Started: {questName}, Progress reset ({currentProgress}/{targetProgress})");

            OnQuestUpdated?.Invoke(this);
        }

        public virtual void UpdateProgress(int amount = 1)
        {
            if (!isActive || isCompleted) return;

            currentProgress += amount;

            if (currentProgress > targetProgress)
                currentProgress = targetProgress;

            Debug.Log($"[Quest] Progress: {questName} ({currentProgress}/{targetProgress})");

            if (currentProgress >= targetProgress)
            {
                CompleteQuest();
            }
            else
            {
                OnQuestUpdated?.Invoke(this);
            }
        }

        public virtual void CompleteQuest()
        {
            if (isCompleted) return; 

            isCompleted = true;
            isActive = false;

            currentProgress = targetProgress;

            Debug.Log($"[Quest] Completed: {questName}");
            OnQuestUpdated?.Invoke(this);
        }

        public virtual void ResetQuest()
        {
            isCompleted = false;
            isActive = false;
            currentProgress = 0;
            OnQuestUpdated?.Invoke(this);
        }
    }
}
