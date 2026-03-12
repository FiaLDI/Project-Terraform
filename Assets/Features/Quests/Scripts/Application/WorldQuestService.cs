using System.Collections.Generic;
using Features.Quests.Domain;

public sealed class WorldQuestService
{
    private readonly QuestService service = new();

    public void HandleEvent(IQuestEvent e)
    {
        service.HandleEvent(e);
    }

    public IReadOnlyCollection<QuestRuntime> ActiveQuests
        => service.ActiveQuests;
}