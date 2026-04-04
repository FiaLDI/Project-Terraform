using UnityEngine;

namespace Features.Quests.Domain
{
    public sealed class ItemRemovedEvent : IQuestEvent
    {
        public GameObject Source { get; }
        public string ItemId { get; }
        public int Amount { get; }

        public ItemRemovedEvent(GameObject source, string itemId, int amount)
        {
            Source = source;
            ItemId = itemId;
            Amount = amount;
        }
    }
}