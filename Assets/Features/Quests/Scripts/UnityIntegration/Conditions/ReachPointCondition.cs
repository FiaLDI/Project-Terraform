using Features.Quests.Domain;

public sealed class ReachPointCondition : IQuestCondition
{
    private readonly string pointId;

    public ReachPointCondition(string pointId)
    {
        this.pointId = pointId;
    }

    public void OnStart(QuestRuntime quest)
    {
        quest.SetTarget(this, 1);
    }

    public void OnEvent(QuestRuntime quest, IQuestEvent e)
    {
        if (e is not PointReachedEvent ev)
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