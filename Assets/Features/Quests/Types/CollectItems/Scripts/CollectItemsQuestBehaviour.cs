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

            foreach (var req in requirements)
            {
                req.currentAmountCollected = 0;
            }

            CalculateQuestProgress();
            quest.NotifyQuestUpdated();

            InventoryManager.OnItemAdded += OnItemAdded;
        }

        private void OnItemAdded(Item item, int quantity)
        {
            if (!active || completed || item == null) return;

            ItemRequirement req = requirements.FirstOrDefault(r => r.itemToCollect.id == item.id);

            if (req != null)
            {
                int needed = req.amountRequired - req.currentAmountCollected;
                if (needed <= 0) return;

                int amountToCredit = Mathf.Min(quantity, needed);
                req.currentAmountCollected += amountToCredit;

                Debug.Log($"Квест '{myQuest.questName}': засчитано {amountToCredit}х {item.itemName}");

                CalculateQuestProgress();
                myQuest.NotifyQuestUpdated();

                if (myQuest.IsCompleted)
                {
                    CompleteQuest(myQuest);
                }
            }
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

        public override void CompleteQuest(QuestAsset quest)
        {
            if (completed) return;
            completed = true;
            active = false;
            InventoryManager.OnItemAdded -= OnItemAdded;
        }

        public override void ResetQuest(QuestAsset quest)
        {
            active = false;
            completed = false;
            myQuest = null;

            foreach (var req in requirements)
            {
                req.currentAmountCollected = 0;
            }

            InventoryManager.OnItemAdded -= OnItemAdded;
            CalculateQuestProgress();
        }

        public override bool IsActive => active;
        public override bool IsCompleted => completed;
        public override int CurrentProgress => myQuest != null ? myQuest.currentProgress : 0;
        public override int TargetProgress => myQuest != null ? myQuest.targetProgress : 0;

        public override QuestBehaviour Clone()
        {
            var clone = (CollectItemsQuestBehaviour)MemberwiseClone();

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
            return clone;
        }
    }
}