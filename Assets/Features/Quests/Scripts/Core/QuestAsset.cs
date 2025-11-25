using UnityEngine;
using System;

namespace Quests
{
    [CreateAssetMenu(menuName = "Quests/QuestAsset")]
    public class QuestAsset : ScriptableObject
    {
        public string questName;
        [TextArea] public string description;
        public bool alreadyCompleted = false;


        [SerializeReference] 
        public QuestBehaviour behaviour;

        public int currentProgress;
        public int targetProgress;

        public event Action<QuestAsset> OnQuestUpdated;

        [Header("Rewards")]
        public RewardItem[] rewards;

        public bool IsCompleted => currentProgress >= targetProgress && targetProgress > 0;

        public void ResetProgress()
        {
            currentProgress = 0;
            // targetProgress НЕ меняем — заранее задано вручную
        }

        public void AddProgress(int amount)
        {
            currentProgress += amount;
            NotifyQuestUpdated();
        }

        public void NotifyQuestUpdated()
        {
            OnQuestUpdated?.Invoke(this);
        }

        public void GiveRewards()
        {
            if (rewards == null || rewards.Length == 0) return;

            foreach (var r in rewards)
            {
                if (r.item != null && r.amount > 0)
                    InventoryManager.instance.AddItem(r.item, r.amount);
            }
        }


        public void TargetCompleted()
        {
            AddProgress(1);
        }
    }

    [Serializable]
    public class RewardItem
    {
        public Item item;
        public int amount;
    }
}
