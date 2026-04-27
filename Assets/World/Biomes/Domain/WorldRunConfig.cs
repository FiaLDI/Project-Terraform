using UnityEngine;

public sealed class WorldRunConfig
{
    public string worldConfigId = string.Empty;
    public int worldLevel = 1;
    public int difficulty = 2;

    public float levelScale = 1f;
    public float enemyStatScale = 1f;
    public float enemySpawnScale = 1f;
    public float rewardScale = 1f;
    public float progressFraction = 0.4f;

    public WorldRunConfig Clone()
    {
        return new WorldRunConfig
        {
            worldConfigId = worldConfigId,
            worldLevel = worldLevel,
            difficulty = difficulty,
            levelScale = levelScale,
            enemyStatScale = enemyStatScale,
            enemySpawnScale = enemySpawnScale,
            rewardScale = rewardScale,
            progressFraction = progressFraction
        };
    }

    public int GetCompletionExperience(int playerLevel)
    {
        int requiredExperience =
            PlayerProgressionRules.GetRequiredExperienceForLevel(playerLevel);

        float normalizedFraction = Mathf.Clamp01(progressFraction);
        return Mathf.Max(1, Mathf.RoundToInt(requiredExperience * normalizedFraction));
    }
}

public static class WorldRunContext
{
    public static WorldRunConfig Current { get; private set; }

    public static void Set(WorldRunConfig config)
    {
        Current = config?.Clone();
    }

    public static void Clear()
    {
        Current = null;
    }
}
