using UnityEngine;
using System;
using System.Collections.Generic;
using Features.Quests.Domain;
using Features.Quests.Domain.Behaviours;
using Features.Enemy.Data;

namespace Features.Quests.Data
{
    [CreateAssetMenu(menuName = "Quests/QuestAsset")]
    public class QuestAsset : ScriptableObject
    {
        [Header("ID и тексты")]
        public string questId;
        public string questName;
        [TextArea] public string description;

        [Header("Тип поведения")]
        public QuestBehaviourType behaviourType;

        [Header("Enemy Database")]
        public EnemyDatabaseSO enemyDatabase;
        public string enemyId;
        public int requiredKills = 5;

        [Header("Параметры CollectItems")]
        public ItemRequirementConfig[] itemRequirements;

        [Header("Параметры Point/Interact/StandOnPoint")]
        public string pointId;
        public float requiredStayTime = 2f;

        [Header("Награды")]
        public RewardItemConfig[] rewards;

        public QuestDefinition ToDefinition()
        {
            IQuestBehaviour behaviour = behaviourType switch
            {
                QuestBehaviourType.KillEnemies =>
                    new KillEnemiesQuestBehaviour(enemyId, requiredKills),

                QuestBehaviourType.CollectItems =>
                    new CollectItemsQuestBehaviour(
                        BuildRequirementsForDomain()
                    ),

                QuestBehaviourType.ReachPoint =>
                    new ReachPointQuestBehaviour(pointId),

                QuestBehaviourType.InteractPoint =>
                    new InteractPointQuestBehaviour(pointId),

                QuestBehaviourType.StandOnPoint =>
                    new StandOnPointQuestBehaviour(pointId, requiredStayTime),

                _ => throw new ArgumentOutOfRangeException()
            };

            var rewardsDomain = new List<QuestReward>();
            if (rewards != null)
            {
                foreach (var r in rewards)
                {
                    if (string.IsNullOrEmpty(r.itemId) || r.amount <= 0)
                        continue;

                    rewardsDomain.Add(new QuestReward(r.itemId, r.amount));
                }
            }

            return new QuestDefinition(
                new QuestId(questId),
                questName,
                description,
                behaviour,
                rewardsDomain
            );
        }

        private IEnumerable<CollectItemsQuestBehaviour.Requirement> BuildRequirementsForDomain()
        {
            if (itemRequirements == null)
                yield break;

            foreach (var req in itemRequirements)
            {
                if (string.IsNullOrEmpty(req.itemId) || req.amountRequired <= 0)
                    continue;

                yield return new CollectItemsQuestBehaviour.Requirement(
                    req.itemId,
                    req.amountRequired
                );
            }
        }
    }

    public enum QuestBehaviourType
    {
        KillEnemies,
        CollectItems,
        ReachPoint,
        InteractPoint,
        StandOnPoint,
        ProceduralPoints
    }

    [Serializable]
    public class RewardItemConfig
    {
        public string itemId;     // вместо прямого Item
        public int amount;
    }

    [Serializable]
    public class ItemRequirementConfig
    {
        public string itemId;
        public int amountRequired;
    }
}
