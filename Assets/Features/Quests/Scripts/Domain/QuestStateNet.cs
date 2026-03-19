using FishNet.Serializing;

public struct QuestNetState
{
    public string questId;
    public QuestConditionNetState[] conditions;
    public bool completed;

    public QuestNetState(string id, QuestConditionNetState[] conditions, bool completed)
    {
        this.questId = id;
        this.conditions = conditions;
        this.completed = completed;
    }
}