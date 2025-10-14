using UnityEngine;
using System.Collections.Generic;

namespace Quests
{
    [CreateAssetMenu(fileName = "NewQuestChain", menuName = "Quests/Quest Chain")]
    public class QuestChain : ScriptableObject
    {
        [Header("Настройки цепочки квестов")]
        public string chainName;
        public string chainDescription;

        [Header("Квесты в цепочке (по порядку)")]
        public List<Quest> questsInOrder = new List<Quest>();

        [Header("Автозапуск")]
        public bool autoStartFirstQuest = true;

        [Header("Текущий прогресс")]
        [SerializeField] private int currentQuestIndex = 0;

        public Quest GetCurrentQuest()
        {
            if (currentQuestIndex < questsInOrder.Count && currentQuestIndex >= 0)
                return questsInOrder[currentQuestIndex];
            return null;
        }

        public void StartChain()
        {
            Debug.Log($"?? QuestChain.StartChain: Запуск цепочки '{chainName}'");
            currentQuestIndex = 0;
            StartCurrentQuest();
        }

        public void MoveToNextQuest()
        {
            Debug.Log($"?? QuestChain.MoveToNextQuest: Переход к следующему квесту. Текущий индекс: {currentQuestIndex}, Всего квестов: {questsInOrder.Count}");

            currentQuestIndex++;
            if (currentQuestIndex < questsInOrder.Count)
            {
                Debug.Log($"?? QuestChain: Переходим к квесту {currentQuestIndex + 1}/{questsInOrder.Count}");
                StartCurrentQuest();
            }
            else
            {
                Debug.Log($"?? QuestChain: Цепочка квестов '{chainName}' завершена!");
            }
        }

        private void StartCurrentQuest()
        {
            Quest currentQuest = GetCurrentQuest();
            if (currentQuest != null)
            {
                Debug.Log($"?? QuestChain: Запускаем квест {currentQuestIndex + 1}/{questsInOrder.Count}: '{currentQuest.questName}'");

                if (QuestManager.Instance != null)
                {
                    QuestManager.Instance.StartQuest(currentQuest);
                }
                else
                {
                    Debug.LogError("? QuestChain: QuestManager.Instance is null!");
                }
            }
            else
            {
                Debug.LogError($"? QuestChain: Не удалось найти квест с индексом {currentQuestIndex}");
            }
        }

        public void ResetChain()
        {
            currentQuestIndex = 0;
            foreach (Quest quest in questsInOrder)
            {
                if (quest != null) quest.ResetQuest();
            }
            Debug.Log($"?? QuestChain: Цепочка '{chainName}' сброшена");
        }
    }
}