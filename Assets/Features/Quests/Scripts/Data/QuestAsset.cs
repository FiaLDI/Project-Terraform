using UnityEngine;
using System;
using System.Collections.Generic;
using Features.Quests.Domain;

namespace Features.Quests.Data
{
    [CreateAssetMenu(menuName = "Quests/QuestAsset")]
    public class QuestAsset : ScriptableObject
    {
        [Header("ID")]
        public string questId;

        public string questName;

        [TextArea]
        public string description;

        [Header("Scope")]
        public QuestScope scope = QuestScope.Personal;

        [Header("Conditions")]

        public EnemyKillConditionConfig[] killEnemies;

        public ItemCollectConditionConfig[] collectItems;

        public string reachPointId;

        [Header("Rewards")]

        public RewardItemConfig[] rewards;

        public QuestDefinition ToDefinition()
        {
            var conditions = new List<IQuestCondition>();

            // Kill enemies
            if (killEnemies != null)
            {
                foreach (var c in killEnemies)
                {
                    if (string.IsNullOrEmpty(c.enemyId))
                        continue;

                    conditions.Add(
                        new KillEnemyCondition(
                            c.enemyId,
                            c.requiredKills
                        )
                    );
                }
            }

            // Collect items
            if (collectItems != null)
            {
                foreach (var c in collectItems)
                {
                    if (string.IsNullOrEmpty(c.itemId))
                        continue;

                    conditions.Add(
                        new CollectItemCondition(
                            c.itemId,
                            c.requiredAmount
                        )
                    );
                }
            }

            // Reach point
            if (!string.IsNullOrEmpty(reachPointId))
            {
                conditions.Add(
                    new ReachPointCondition(reachPointId)
                );
            }

            var rewardsDomain = new List<QuestReward>();

            if (rewards != null)
            {
                foreach (var r in rewards)
                {
                    if (string.IsNullOrEmpty(r.itemId))
                        continue;

                    if (r.amount <= 0)
                        continue;

                    rewardsDomain.Add(
                        new QuestReward(r.itemId, r.amount)
                    );
                }
            }

            return new QuestDefinition(
                new QuestId(questId),
                questName,
                description,
                scope,
                conditions,
                rewardsDomain
            );
        }
    }

    [Serializable]
    public class EnemyKillConditionConfig
    {
        public string enemyId;
        public int requiredKills;
    }

    [Serializable]
    public class ItemCollectConditionConfig
    {
        public string itemId;
        public int requiredAmount;
    }

    [Serializable]
    public class RewardItemConfig
    {
        public string itemId;
        public int amount;
    }
}
