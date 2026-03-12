using Features.Quests.Domain;

public sealed class KillEnemyCondition : IQuestCondition
{
    private readonly string enemyId;
    private readonly int required;

    public KillEnemyCondition(string enemyId, int required)
    {
        this.enemyId = enemyId;
        this.required = required;
    }

    public void OnStart(QuestRuntime quest)
    {
        quest.SetTarget(this, required);
    }

    public void OnEvent(QuestRuntime quest, IQuestEvent e)
    {
        if (e is not EnemyKilledEvent ev)
            return;

        if (ev.EnemyId != enemyId)
            return;

        quest.AddProgress(this, 1);
    }

    public bool IsCompleted(QuestRuntime quest)
    {
        return quest.GetProgress(this) >= required;
    }
}
