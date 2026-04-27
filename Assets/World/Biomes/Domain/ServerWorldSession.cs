
using System.Collections.Generic;

public static class ServerWorldSession
{
    public static int PendingSeed;
    public static string PendingWorldConfigId = string.Empty;
    public static WorldRunConfig PendingRunConfig;
    public static List<string> PendingQuestIds = new();
    public static List<string> PendingChainIds = new();

    public static (int seed, WorldRunConfig runConfig) ConsumeBootstrap()
    {
        WorldRunConfig runConfig =
            PendingRunConfig?.Clone() ??
            WorldRunBalance.CreateDefault(PendingWorldConfigId, 1);

        var result = (
            PendingSeed,
            runConfig
        );

        ResetPendingWorldBootstrap();

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

    public static void SetPendingRunConfig(WorldRunConfig runConfig)
    {
        PendingRunConfig = runConfig?.Clone();
        PendingWorldConfigId = PendingRunConfig != null
            ? PendingRunConfig.worldConfigId ?? string.Empty
            : string.Empty;
    }

    public static void ResetPendingWorldBootstrap()
    {
        PendingSeed = 0;
        PendingWorldConfigId = string.Empty;
        PendingRunConfig = null;
    }

    public static void ResetPendingQuestBootstrap()
    {
        PendingQuestIds.Clear();
        PendingChainIds.Clear();
    }
}
