using System;

[Serializable]
public struct GlobalStatModifier
{
    public ItemStatType stat;
    public float flatBonus;     // +3 damage
    public float percentBonus;  // +0.15 → +15%
}
