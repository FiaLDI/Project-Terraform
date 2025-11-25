using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Quests
{
    [System.Serializable]
    public class ItemRequirement
    {
        public Item itemToCollect;
        public int amountRequired;

        [HideInInspector]
        public int currentAmountCollected;
    }

    [System.Serializable]
    public class CollectItemsQuestBehaviour : QuestBehaviour
    {
        [Header("Список требуемых предметов")]
        public List<ItemRequirement> requirements = new List<ItemRequirement>();

        private bool active;
        private bool completed;
        private QuestAsset myQuest;

        public override void StartQuest(QuestAsset quest)
        {
            myQuest = quest;
            active = true;
            completed = false;

            // Обнуляем прогресс по предметам
            foreach (var req in requirements)
                req.currentAmountCollected = 0;

            // Считаем стартовый прогресс
            CalculateQuestProgress();
            myQuest?.NotifyQuestUpdated();

            // Подписываемся на события инвентаря
            InventoryManager.OnItemAdded -= OnItemAdded;
            InventoryManager.OnItemAdded += OnItemAdded;
        }

        private void OnItemAdded(Item item, int quantity)
        {
            if (!active || completed || item == null || myQuest == null)
                return;

            // Находим требуемый предмет по ссылке или по id
            ItemRequirement req = requirements.FirstOrDefault(r =>
                r.itemToCollect == item ||
                (r.itemToCollect != null && r.itemToCollect.id == item.id));

            if (req == null)
                return;

            int needed = req.amountRequired - req.currentAmountCollected;
            if (needed <= 0) return;

            int amountToCredit = Mathf.Min(quantity, needed);
            req.currentAmountCollected += amountToCredit;

            Debug.Log($"Квест '{myQuest.questName}': засчитано {amountToCredit}× {item.itemName}");

            // Пересчёт прогресса квеста
            CalculateQuestProgress();
            myQuest.NotifyQuestUpdated();

            // Сообщаем менеджеру, что прогресс изменился
            QuestManager.Instance?.UpdateQuestProgress(myQuest);
        }

        private void CalculateQuestProgress()
        {
            if (myQuest == null) return;

            int current = 0;
            int target = 0;

            foreach (var req in requirements)
            {
                current += req.currentAmountCollected;
                target += req.amountRequired;
            }

            myQuest.currentProgress = current;
            myQuest.targetProgress = target;
        }

        public override void UpdateProgress(QuestAsset quest, int amount = 1)
        {
            // Здесь логика минимальная: прогресс уже пересчитан в OnItemAdded.
            // Но на всякий случай пересчитаем ещё раз (идемпотентно).
            if (!active || completed || myQuest == null)
                return;

            CalculateQuestProgress();
        }

        public override void CompleteQuest(QuestAsset quest)
        {
            if (completed) return;
            completed = true;
            active = false;

            InventoryManager.OnItemAdded -= OnItemAdded;
            myQuest?.NotifyQuestUpdated();
        }

        public override void ResetQuest(QuestAsset quest)
        {
            active = false;
            completed = false;
            myQuest = null;

            foreach (var req in requirements)
                req.currentAmountCollected = 0;

            InventoryManager.OnItemAdded -= OnItemAdded;

            if (quest != null)
            {
                quest.currentProgress = 0;
                quest.targetProgress = 0;
                quest.NotifyQuestUpdated();
            }
        }

        public override bool IsActive => active;
        public override bool IsCompleted => completed;
        public override int CurrentProgress => myQuest != null ? myQuest.currentProgress : 0;
        public override int TargetProgress => myQuest != null ? myQuest.targetProgress : 0;

        public override QuestBehaviour Clone()
        {
            var clone = (CollectItemsQuestBehaviour)MemberwiseClone();

            // Глубокая копия списка требований
            clone.requirements = new List<ItemRequirement>();
            foreach (var req in requirements)
            {
                clone.requirements.Add(new ItemRequirement
                {
                    itemToCollect = req.itemToCollect,
                    amountRequired = req.amountRequired,
                    currentAmountCollected = 0
                });
            }

            // У клона ещё нет myQuest, он будет установлен в StartQuest
            return clone;
        }
    }
}
