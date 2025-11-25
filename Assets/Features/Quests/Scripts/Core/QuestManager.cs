using System.Collections.Generic;
using UnityEngine;

namespace Quests
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance;

        public List<QuestAsset> activeQuests = new();
        public List<QuestAsset> completedQuests = new();

        public QuestUI questUI;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartQuest(QuestAsset quest)
        {
            if (quest == null) return;
            if (activeQuests.Contains(quest) || completedQuests.Contains(quest))
                return;

            quest.ResetProgress();
            quest.behaviour?.StartQuest(quest);

            activeQuests.Add(quest);
            questUI?.AddQuest(quest);
            quest.NotifyQuestUpdated();
        }

        public void UpdateQuestProgress(QuestAsset quest, int amount = 1)
        {
            if (quest == null) return;

            if (quest.IsCompleted)
                return;

            quest.AddProgress(amount);
            questUI?.UpdateQuest(quest);

            if (quest.IsCompleted)
                CompleteQuest(quest);
        }


        public void CompleteQuest(QuestAsset quest)
        {
            if (quest.alreadyCompleted)
                return;

            if (activeQuests.Remove(quest))
                completedQuests.Add(quest);

            quest.NotifyQuestUpdated();

            questUI?.RemoveQuest(quest);
            quest.GiveRewards();
        }
    }
}
