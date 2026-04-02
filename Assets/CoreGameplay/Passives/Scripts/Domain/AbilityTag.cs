[System.Flags]
public enum AbilityTag
{
    None        = 0,
    Projectile  = 1 << 0,
    Fire        = 1 << 1,
    Dash        = 1 << 2,
    AOE         = 1 << 3
}
