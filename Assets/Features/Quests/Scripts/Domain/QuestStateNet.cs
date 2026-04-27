using FishNet.Serializing;
using Features.Quests.Domain;

public struct QuestNetState
{
    public string questId;
    public QuestConditionNetState[] conditions;
    public QuestState state;
    public bool completed;

    public QuestNetState(string id, QuestConditionNetState[] conditions, QuestState state)
    {
        this.questId = id;
        this.conditions = conditions;
        this.state = state;
        this.completed = state == QuestState.Completed;
    }
}
