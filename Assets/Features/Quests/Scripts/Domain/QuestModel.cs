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

    public readonly struct QuestId : IEquatable<QuestId>
    {
        public string Value { get; }

        public QuestId(string value)
        {
            Value = value;
        }

        public bool Equals(QuestId other) => Value == other.Value;

        public override bool Equals(object obj)
            => obj is QuestId other && Equals(other);

        public override int GetHashCode()
            => Value != null ? Value.GetHashCode() : 0;

        public override string ToString() => Value;
    }

    public enum QuestScope
    {
        Personal,
        Shared
    }

    public sealed class QuestReward
    {
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
        public QuestScope Scope { get; }

        public IReadOnlyList<IQuestCondition> Conditions { get; }

        public IReadOnlyList<QuestReward> Rewards { get; }

        public QuestDefinition(
            QuestId id,
            string name,
            string description,
            QuestScope scope,
            IReadOnlyList<IQuestCondition> conditions,
            IReadOnlyList<QuestReward> rewards)
        {
            Id = id;
            Name = name;
            Description = description;
            Scope = scope;
            Conditions = conditions;
            Rewards = rewards ?? Array.Empty<QuestReward>();
        }
    }
}
