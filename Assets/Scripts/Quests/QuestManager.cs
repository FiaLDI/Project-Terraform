using System.Collections.Generic;
using UnityEngine;

namespace Quests
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance;

        [Header("Активные квесты")]
        public List<Quest> activeQuests = new List<Quest>();

        [Header("Завершённые квесты")]
        public List<Quest> completedQuests = new List<Quest>(); 

        [Header("UI-компонент для отображения квестов")]
        public QuestUI questUI;

        [Header("Начальный квест")]
        public Quest startingQuest;

        [Header("Менеджер цепочек квестов")]
        public QuestChainManager chainManager;

        private bool isCompletingQuest = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Debug.Log("? QuestManager: Instance создан");
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Debug.Log("?? QuestManager: Start() вызван");

            if (chainManager == null)
            {
                Debug.LogError("? QuestManager: chainManager не назначен!");
            }
            else
            {
                Debug.Log("? QuestManager: chainManager назначен");
            }

            if (startingQuest != null)
            {
                StartQuest(startingQuest);
                Debug.Log($"? QuestManager: Автозапуск квеста: {startingQuest.questName}");
            }
            else
            {
                Debug.Log("?? QuestManager: startingQuest не назначен");
            }
        }

        public void StartQuest(Quest quest)
        {
            if (quest == null)
            {
                Debug.LogError("? QuestManager.StartQuest: quest is null!");
                return;
            }

            if (activeQuests.Contains(quest))
            {
                Debug.LogWarning($"?? QuestManager.StartQuest: Квест '{quest.questName}' уже активен");
                return;
            }

            Debug.Log($"?? QuestManager.StartQuest: ЗАПУСКАЕМ '{quest.questName}'");

            quest.StartQuest();
            quest.OnQuestUpdated += HandleQuestUpdated;
            activeQuests.Add(quest);

            questUI?.AddQuest(quest);
        }

        public void UpdateQuestProgress(Quest quest, int amount = 1)
        {
            if (quest == null)
            {
                Debug.LogError("? QuestManager.UpdateQuestProgress: quest is null!");
                return;
            }

            if (!activeQuests.Contains(quest))
            {
                Debug.LogError($"? QuestManager.UpdateQuestProgress: Квест '{quest.questName}' не активен!");
                return;
            }

            Debug.Log($"?? QuestManager.UpdateQuestProgress: {quest.questName} +{amount}");

            quest.UpdateProgress(amount);
        }

        public void CompleteQuest(Quest quest)
        {
            if (isCompletingQuest)
            {
                Debug.LogWarning($"?? QuestManager.CompleteQuest: Уже завершаем другой квест, пропускаем '{quest?.questName}'");
                return;
            }

            if (quest == null)
            {
                Debug.LogError("? QuestManager.CompleteQuest: quest is null!");
                return;
            }

            if (!activeQuests.Contains(quest))
            {
                Debug.LogError($"? QuestManager.CompleteQuest: Квест '{quest.questName}' не активен!");
                return;
            }

            isCompletingQuest = true;

            try
            {
                Debug.Log($"?? QuestManager.CompleteQuest: ЗАВЕРШАЕМ '{quest.questName}'");

                quest.OnQuestUpdated -= HandleQuestUpdated;

                quest.CompleteQuest();
                activeQuests.Remove(quest);

                if (!completedQuests.Contains(quest))
                    completedQuests.Add(quest);

                questUI?.UpdateQuest(quest);

                if (chainManager != null)
                {
                    chainManager.OnQuestCompleted(quest);
                }
            }
            finally
            {
                isCompletingQuest = false;
            }
        }

        private void HandleQuestUpdated(Quest quest)
        {
            if (quest == null)
            {
                Debug.LogError("QuestManager.HandleQuestUpdated: quest is null");
                return;
            }

            Debug.Log($"?? QuestManager.HandleQuestUpdated: {quest.questName} (Active: {quest.isActive}, Completed: {quest.isCompleted})");

            questUI?.UpdateQuest(quest);

            if (quest.isCompleted)
            {
                if (activeQuests.Contains(quest))
                {
                    CompleteQuest(quest);
                }
            }
        }
    }
}
