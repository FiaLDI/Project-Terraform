using FishNet.Serializing;

public struct QuestNetState
{
    public string questId;
    public int progress;
    public int target;
    public bool completed;

    public QuestNetState(string id, int progress, int target, bool completed)
    {
        this.questId = id;
        this.progress = progress;
        this.target = target;
        this.completed = completed;
    }
}
