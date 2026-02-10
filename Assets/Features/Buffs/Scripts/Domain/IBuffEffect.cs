using Features.Stats.Domain;

namespace Features.Buffs.Domain
{
    public interface IBuffEffect
    {
        void Apply(IStatsFacade stats);

        void Tick(IStatsFacade stats, float dt);

        void Expire(IStatsFacade stats);
    }
}
