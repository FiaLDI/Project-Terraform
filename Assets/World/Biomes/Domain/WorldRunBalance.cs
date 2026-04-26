using UnityEngine;

public static class WorldRunBalance
{
    public const int DefaultDifficulty = 2;
    public const int MinDifficulty = 1;
    public const int MaxDifficulty = 5;

    private static readonly float[] DifficultyStatScales =
    {
        0.8f,
        1f,
        1.2f,
        1.5f,
        2f
    };

    private static readonly float[] DifficultySpawnScales =
    {
        0.8f,
        1f,
        1.15f,
        1.3f,
        1.6f
    };

    private static readonly float[] DifficultyRewardScales =
    {
        0.8f,
        1f,
        1.25f,
        1.6f,
        2.2f
    };

    private static readonly float[] DifficultyProgressFractions =
    {
        0.35f,
        0.4f,
        0.5f,
        0.6f,
        0.7f
    };

    public static WorldRunConfig Create(string worldConfigId, int worldLevel, int difficulty)
    {
        int normalizedLevel = PlayerProgressionRules.NormalizeLevel(worldLevel);
        int normalizedDifficulty = ClampDifficulty(difficulty);
        int index = normalizedDifficulty - 1;

        float levelScale = GetLevelScale(normalizedLevel);

        return new WorldRunConfig
        {
            worldConfigId = worldConfigId ?? string.Empty,
            worldLevel = normalizedLevel,
            difficulty = normalizedDifficulty,
            levelScale = levelScale,
            enemyStatScale = levelScale * DifficultyStatScales[index],
            enemySpawnScale = DifficultySpawnScales[index],
            rewardScale = DifficultyRewardScales[index],
            progressFraction = DifficultyProgressFractions[index]
        };
    }

    public static WorldRunConfig CreateDefault(string worldConfigId, int worldLevel = 1)
    {
        return Create(worldConfigId, worldLevel, DefaultDifficulty);
    }

    public static int ClampDifficulty(int difficulty)
    {
        return Mathf.Clamp(difficulty, MinDifficulty, MaxDifficulty);
    }

    public static string GetDifficultyLabel(int difficulty)
    {
        switch (ClampDifficulty(difficulty))
        {
            case 1:
                return "Safe";
            case 2:
                return "Normal";
            case 3:
                return "Hard";
            case 4:
                return "Dangerous";
            case 5:
                return "Elite";
            default:
                return "Normal";
        }
    }

    private static float GetLevelScale(int worldLevel)
    {
        return 1f + (worldLevel - 1) * 0.1f;
    }
}
