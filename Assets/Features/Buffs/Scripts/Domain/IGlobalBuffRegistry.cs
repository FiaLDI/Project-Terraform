namespace Features.Buffs.Domain
{
    public interface IGlobalBuffRegistry
    {
        float GetValue(string key);
        void Add(string key, float value);
        void Remove(string key, float value);
    }
}
