using UnityEngine;

public static class GlobalMiningSpeed
{
    public static float Multiplier { get; private set; } = 1f;

    public static void SetMultiplier(float value)
    {
        Multiplier = Mathf.Max(0.1f, value);
    }

    public static void AddBonus(float bonus)
    {
        Multiplier += bonus;
    }
}
