
using System.Collections.Generic;

public static class ServerWorldSession
{
    public static int PendingSeed;
    public static List<string> PendingQuestIds = new();
    public static List<string> PendingChainIds = new();

    public static (int seed, List<string> quests, List<string> chains) Consume()
    {
        var result = (
            PendingSeed,
            PendingQuestIds,
            PendingChainIds
        );

        PendingSeed = 0;
        PendingQuestIds = new();
        PendingChainIds = new();

        return result;
    }
}