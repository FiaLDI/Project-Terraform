using UnityEngine;

public static class PlayerProgressionRules
{
    private const int BaseExperienceToLevel = 100;
    private const int ExperienceStepPerLevel = 50;

    public static int NormalizeLevel(int level)
    {
        return Mathf.Max(1, level);
    }

    public static int NormalizeExperience(int experience)
    {
        return Mathf.Max(0, experience);
    }

    public static int GetRequiredExperienceForLevel(int level)
    {
        int normalizedLevel = NormalizeLevel(level);
        return BaseExperienceToLevel + (normalizedLevel - 1) * ExperienceStepPerLevel;
    }

    public static float GetProgress01(int level, int experience)
    {
        int required = GetRequiredExperienceForLevel(level);
        if (required <= 0)
            return 0f;

        return Mathf.Clamp01(NormalizeExperience(experience) / (float)required);
    }

    public static void ApplyExperience(ref int level, ref int experience, int gainedExperience)
    {
        level = NormalizeLevel(level);
        experience = NormalizeExperience(experience);

        if (gainedExperience <= 0)
            return;

        experience += gainedExperience;

        int required = GetRequiredExperienceForLevel(level);
        while (experience >= required)
        {
            experience -= required;
            level++;
            required = GetRequiredExperienceForLevel(level);
        }
    }
}
