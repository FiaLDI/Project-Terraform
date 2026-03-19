

namespace Features.Quests.Domain
{
    public sealed class ItemAddedEvent : IQuestEvent
    {
        public string ItemId { get; }
        public int Amount { get; }

        public ItemAddedEvent(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }
}
