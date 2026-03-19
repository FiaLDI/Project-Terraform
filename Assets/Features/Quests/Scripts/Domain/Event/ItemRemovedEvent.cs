
namespace Features.Quests.Domain
{
    public sealed class ItemRemovedEvent : IQuestEvent
    {
        public string ItemId { get; }
        public int Amount { get; }

        public ItemRemovedEvent(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }
}
