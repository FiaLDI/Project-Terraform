
using System.Collections.Generic;

public static class ServerWorldSession
{
    public static int PendingSeed;
    public static string PendingWorldConfigId = string.Empty;
    public static List<string> PendingQuestIds = new();
    public static List<string> PendingChainIds = new();

    public static (int seed, string worldConfigId) ConsumeBootstrap()
    {
        var result = (
            PendingSeed,
            PendingWorldConfigId
        );

        PendingSeed = 0;
        PendingWorldConfigId = string.Empty;

        return result;
    }

    public static (List<string> questIds, List<string> chainIds) ConsumeQuestBootstrap()
    {
        var result = (
            new List<string>(PendingQuestIds),
            new List<string>(PendingChainIds)
        );

        PendingQuestIds.Clear();
        PendingChainIds.Clear();

        return result;
    }
}
