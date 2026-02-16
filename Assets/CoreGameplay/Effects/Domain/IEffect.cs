namespace Features.Effects.Domain
{
    public interface IEffect
    {
        void Apply(EffectContext context);
    }
}
