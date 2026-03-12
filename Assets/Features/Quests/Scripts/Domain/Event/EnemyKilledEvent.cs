using Features.Quests.Domain;

public struct EnemyKilledEvent : IQuestEvent
{
    public string EnemyId;

    public EnemyKilledEvent(string enemyId)
    {
        EnemyId = enemyId;
    }
}
