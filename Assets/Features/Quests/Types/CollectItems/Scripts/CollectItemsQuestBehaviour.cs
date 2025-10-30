// Features/Quests/Types/CollectItems/Scripts/CollectItemsQuestBehaviour.cs
using UnityEngine;

namespace Quests
{
    [System.Serializable]
    public class CollectItemsQuestBehaviour : QuestBehaviour
    {
        [Header("Настройки сбора предметов")]
        public string targetItemId; // ID предмета для сбора
        public int requiredAmount = 1; // Требуемое количество
        public bool specificItemOnly = false; // Только конкретный предмет или любой с этим ID

        private int currentCollected = 0;
        private bool active;
        private bool completed;

        public override void StartQuest(QuestAsset quest)
        {
            currentCollected = 0;
            active = true;
            completed = false;

            // Устанавливаем целевой прогресс
            quest.targetProgress = requiredAmount;
            quest.currentProgress = currentCollected;

            // Подписываемся на события инвентаря
            InventoryManager.OnItemAdded += OnItemAdded;
        }

        public override void UpdateProgress(QuestAsset quest, int amount = 1)
        {
            if (!active || completed) return;

            currentCollected += amount;
            quest.currentProgress = currentCollected;

            if (currentCollected >= requiredAmount)
            {
                CompleteQuest(quest);
            }
        }

        private void OnItemAdded(Item item, int quantity)
        {
            if (!active || completed) return;

            // Проверяем, подходит ли предмет для квеста
            if (item.id.ToString() == targetItemId ||
                (!specificItemOnly && item.itemName.Contains(targetItemId)))
            {
                UpdateProgress(null, quantity);
            }
        }

        public override void CompleteQuest(QuestAsset quest)
        {
            if (completed) return;

            completed = true;
            active = false;
            InventoryManager.OnItemAdded -= OnItemAdded;
        }

        public override void ResetQuest(QuestAsset quest)
        {
            currentCollected = 0;
            active = false;
            completed = false;
            InventoryManager.OnItemAdded -= OnItemAdded;
        }

        public override bool IsActive => active;
        public override bool IsCompleted => completed;
        public override int CurrentProgress => currentCollected;
        public override int TargetProgress => requiredAmount;

        public override QuestBehaviour Clone() => (CollectItemsQuestBehaviour)MemberwiseClone();
    }
}