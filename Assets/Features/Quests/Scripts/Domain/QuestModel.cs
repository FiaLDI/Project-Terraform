using System;
using System.Collections.Generic;

namespace Features.Quests.Domain
{
    public enum QuestState
    {
        Inactive,
        Active,
        Completed,
        Failed
    }

    public readonly struct QuestId
    {
        public string Value { get; }
        public QuestId(string value) => Value = value;
        public override string ToString() => Value;
    }

    public sealed class QuestReward
    {
        // Чтобы не привязываться к Item ScriptableObject:
        public string ItemId { get; }
        public int Amount { get; }

        public QuestReward(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }

    public sealed class QuestDefinition
    {
        public QuestId Id { get; }
        public string Name { get; }
        public string Description { get; }

        public IQuestBehaviour Behaviour { get; }
        public IReadOnlyList<QuestReward> Rewards { get; }

        public QuestDefinition(
            QuestId id,
            string name,
            string description,
            IQuestBehaviour behaviour,
            IReadOnlyList<QuestReward> rewards)
        {
            Id = id;
            Name = name;
            Description = description;
            Behaviour = behaviour;
            Rewards = rewards ?? Array.Empty<QuestReward>();
        }
    }
}
