using Features.Quests.Domain;

public sealed class InteractPointCondition : IQuestCondition
{
    private readonly string pointId;

    public InteractPointCondition(string pointId)
    {
        this.pointId = pointId;
    }

    public string GetDescription()
    {
        return $"Interact with {pointId}";
    }

    public void OnStart(QuestRuntime quest)
    {
        quest.SetTarget(this, 1);
    }

    public void OnEvent(QuestRuntime quest, IQuestEvent e)
    {
        if (e is not InteractionEvent ev)
            return;

        if (ev.PointId != pointId)
            return;

        quest.AddProgress(this, 1);
    }

    public bool IsCompleted(QuestRuntime quest)
    {
        return quest.GetProgress(this) >= 1;
    }
}
