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
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            StartSceneQuests();
        }

        private void Start()
        {
            // ⚡ При загрузке сцены ищем все квесты и активируем их
            
        }

        /// <summary>
        /// Запускаем конкретный квест
        /// </summary>
        public void StartQuest(QuestAsset quest)
        {
            if (quest == null || activeQuests.Contains(quest)) return;

            activeQuests.Add(quest);

            // UI сразу подписываем
            questUI?.AddQuest(quest);

            // запуск поведения
            quest.behaviour?.StartQuest(quest);
        }

        /// <summary>
        /// Обновляем прогресс (дергается из QuestPoint при выполнении цели)
        /// </summary>
        public void UpdateQuestProgress(QuestAsset quest)
        {
            if (quest == null) return;

            quest.behaviour?.UpdateProgress(quest);
            questUI?.UpdateQuest(quest);

            if (quest.IsCompleted)
            {
                quest.behaviour?.CompleteQuest(quest);
                CompleteQuest(quest);
            }
        }

        /// <summary>
        /// Завершение квеста
        /// </summary>
        public void CompleteQuest(QuestAsset quest)
        {
            if (quest == null || !activeQuests.Contains(quest)) return;

            activeQuests.Remove(quest);
            completedQuests.Add(quest);

            questUI?.RemoveQuest(quest);
        }

        /// <summary>
        /// Инициализация квестов, если они расставлены вручную на сцене
        /// </summary>
       public void StartSceneQuests()
        {
            QuestPoint[] points = FindObjectsOfType<QuestPoint>();

            HashSet<QuestAsset> quests = new HashSet<QuestAsset>();
            foreach (var point in points)
            {
                if (point.linkedQuest != null)
                    quests.Add(point.linkedQuest);
            }

            foreach (var quest in quests)
            {
                // 1) сбрасываем прогресс ДО старта квеста
                quest.ResetProgress();

                // 2) активируем квест (подпишем UI)
                StartQuest(quest);

                // 3) UI обновим явно
                questUI?.UpdateQuest(quest);
            }

            Debug.Log($"[QuestManager] Запущено квестов на сцене: {quests.Count}");
        }

    }
}
