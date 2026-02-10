namespace Features.Stats.Domain
{
    public interface IStatModifierTarget
    {
        bool TryAdd(StatKey key, float value);
        bool TryMultiply(StatKey key, float multiplier);
    }
}
