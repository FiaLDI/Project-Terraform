using UnityEngine;

namespace Features.Quests.Domain
{
    public sealed class ItemAddedEvent : IQuestEvent
    {
        public GameObject Source { get; }
        public string ItemId { get; }
        public int Amount { get; }

        public ItemAddedEvent(GameObject source, string itemId, int amount)
        {
            Source = source;
            ItemId = itemId;
            Amount = amount;
        }
    }
}
