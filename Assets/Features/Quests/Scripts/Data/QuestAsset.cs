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
        public ItemHaveConditionConfig[] haveItems;

        public string reachPointId;

        [Header("Rewards")]

        [Min(0)]
        public int experienceReward;

        public RewardItemConfig[] rewards;

        public QuestDefinition ToDefinition()
        {
            var conditions = new List<IQuestCondition>();

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

            if (haveItems != null)
            {
                foreach (var c in haveItems)
                {
                    if (string.IsNullOrEmpty(c.itemId))
                        continue;

                    conditions.Add(
                        new HaveItemCondition(
                            c.itemId,
                            c.requiredAmount
                        )
                    );
                }
            }

            int resolvedExperienceReward = experienceReward > 0
                ? experienceReward
                : Mathf.Max(1, conditions.Count) * 50;

            return new QuestDefinition(
                new QuestId(questId),
                questName,
                description,
                scope,
                resolvedExperienceReward,
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

    [Serializable]
    public class ItemHaveConditionConfig
    {
        public string itemId;
        public int requiredAmount;
    }
}
