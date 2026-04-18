namespace Features.Resources.Domain
{
    public interface IResourceNode : IMineable
    {
        float CurrentHp { get; }
        float MaxHp { get; }

        bool ApplyDamage(float amount);
    }
}
