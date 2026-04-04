using Features.Quests.Domain;

public sealed class CollectItemCondition : IQuestCondition
{
    private readonly string itemId;
    private readonly int required;

    public CollectItemCondition(string itemId, int required)
    {
        this.itemId = itemId;
        this.required = required;
    }

    public string GetDescription()
    {
        return $"Collect {required} {itemId}";
    }

    public void OnStart(QuestRuntime quest)
    {
        quest.SetTarget(this, required);
    }

    public void OnEvent(QuestRuntime quest, IQuestEvent e)
    {
        if (e is not ItemAddedEvent ev)
            return;

        if (ev.ItemId != itemId)
            return;

        quest.AddProgress(this, ev.Amount);
    }

    public bool IsCompleted(QuestRuntime quest)
    {
        return quest.GetProgress(this) >= required;
    }
}
