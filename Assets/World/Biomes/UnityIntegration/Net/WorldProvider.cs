using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class WorldProvider : NetworkBehaviour
{
    public readonly SyncVar<int> Seed = new();
    public readonly SyncVar<string> WorldConfigId = new();
    public readonly SyncVar<int> WorldLevel = new();
    public readonly SyncVar<int> Difficulty = new();
    public readonly SyncVar<float> LevelScale = new();
    public readonly SyncVar<float> EnemyStatScale = new();
    public readonly SyncVar<float> EnemySpawnScale = new();
    public readonly SyncVar<float> RewardScale = new();
    public readonly SyncVar<float> ProgressFraction = new();
    public readonly SyncVar<bool> HasBootstrap = new();
    public readonly SyncVar<bool> IsWorldReady = new();

    public override void OnStartServer()
    {
        base.OnStartServer();

        var session = ServerWorldSession.ConsumeBootstrap();
        var runConfig = session.runConfig ?? WorldRunBalance.CreateDefault(string.Empty, 1);

        Seed.Value = session.seed;
        WorldConfigId.Value = runConfig.worldConfigId;
        WorldLevel.Value = runConfig.worldLevel;
        Difficulty.Value = runConfig.difficulty;
        LevelScale.Value = runConfig.levelScale;
        EnemyStatScale.Value = runConfig.enemyStatScale;
        EnemySpawnScale.Value = runConfig.enemySpawnScale;
        RewardScale.Value = runConfig.rewardScale;
        ProgressFraction.Value = runConfig.progressFraction;
        HasBootstrap.Value = true;
        IsWorldReady.Value = false;
    }

    [Server]
    public void SetWorldReady()
    {
        Debug.Log("[WorldProvider] SetWorldReady called");
        IsWorldReady.Value = true;
    }

    public WorldRunConfig GetRunConfig()
    {
        return new WorldRunConfig
        {
            worldConfigId = WorldConfigId.Value ?? string.Empty,
            worldLevel = PlayerProgressionRules.NormalizeLevel(WorldLevel.Value),
            difficulty = WorldRunBalance.ClampDifficulty(Difficulty.Value),
            levelScale = Mathf.Max(0.1f, LevelScale.Value),
            enemyStatScale = Mathf.Max(0.1f, EnemyStatScale.Value),
            enemySpawnScale = Mathf.Max(0.1f, EnemySpawnScale.Value),
            rewardScale = Mathf.Max(0.1f, RewardScale.Value),
            progressFraction = Mathf.Clamp01(ProgressFraction.Value)
        };
    }
}
