using Features.Quests.Domain;

public interface IQuestCondition
{
    void OnStart(QuestRuntime runtime);

    void OnEvent(QuestRuntime runtime, IQuestEvent e);

    bool IsCompleted(QuestRuntime runtime);
}