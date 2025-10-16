using System.Collections.Generic;
using UnityEngine;

namespace Quests
{
    [CreateAssetMenu(fileName = "NewQuestChain", menuName = "Quests/Quest Chain")]
    public class QuestChain : ScriptableObject
    {
        [Header("Основные данные")]
        public string chainName;

        [Header("Квесты по порядку")]
        public List<QuestAsset> questsInOrder = new List<QuestAsset>();

        private int currentQuestIndex = -1;

        /// <summary>
        /// Запуск цепочки с первого квеста
        /// </summary>
        public void StartChain()
        {
            currentQuestIndex = 0;
            if (questsInOrder.Count > 0 && questsInOrder[0] != null)
            {
                Debug.Log($"▶ QuestChain '{chainName}' стартует с квеста '{questsInOrder[0].questName}'");
                QuestManager.Instance.StartQuest(questsInOrder[0]);
            }
            else
            {
                Debug.LogWarning($"⚠ QuestChain '{chainName}' пустая или первый квест не назначен!");
            }
        }

        /// <summary>
        /// Переход к следующему квесту
        /// </summary>
        public void MoveToNextQuest()
        {
            if (currentQuestIndex < 0 || currentQuestIndex >= questsInOrder.Count)
            {
                Debug.LogWarning($"⚠ QuestChain '{chainName}' уже завершена или не запущена!");
                return;
            }

            currentQuestIndex++;
            if (currentQuestIndex < questsInOrder.Count)
            {
                QuestAsset nextQuest = questsInOrder[currentQuestIndex];
                if (nextQuest != null)
                {
                    Debug.Log($"▶ QuestChain '{chainName}' перешла к следующему квесту: {nextQuest.questName}");
                    QuestManager.Instance.StartQuest(nextQuest);
                }
                else
                {
                    Debug.LogError($"❌ QuestChain '{chainName}': квест на позиции {currentQuestIndex} пустой!");
                }
            }
            else
            {
                Debug.Log($"✅ QuestChain '{chainName}' полностью завершена!");
                currentQuestIndex = -1; // сброс
            }
        }

        /// <summary>
        /// Сброс всей цепочки
        /// </summary>
        public void ResetChain()
        {
            Debug.Log($"⟳ QuestChain '{chainName}' сброшена");
            currentQuestIndex = -1;
        }
    }
}
