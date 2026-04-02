namespace Features.Stats.Domain
{
    public sealed class StatsFacade : IStatsFacade
    {
        public IHealthStats Health { get; }
        public IEnergyStats Energy { get; }
        public ICombatStats Combat { get; }
        public IMovementStats Movement { get; }
        public IMiningStats Mining { get; }
        public IProtectStats Protect { get; private set; }

        private readonly IStatModifierTarget[] _targets;

        public StatsFacade(
            IHealthStats health,
            IEnergyStats energy,
            ICombatStats combat,
            IMovementStats movement,
            IMiningStats mining,
            IProtectStats protect)
        {
            Health = health;
            Energy = energy;
            Combat = combat;
            Movement = movement;
            Mining = mining;
            Protect = protect;

            _targets = new IStatModifierTarget[]
            {
                health as IStatModifierTarget,
                energy as IStatModifierTarget,
                combat as IStatModifierTarget,
                movement as IStatModifierTarget,
                mining as IStatModifierTarget,
                protect as IStatModifierTarget,
            };
        }

        public bool TryAdd(StatKey key, float value)
        {
            foreach (var t in _targets)
                if (t != null && t.TryAdd(key, value))
                    return true;

            return false;
        }

        public bool TryMultiply(StatKey key, float multiplier)
        {
            foreach (var t in _targets)
                if (t != null && t.TryMultiply(key, multiplier))
                    return true;

            return false;
        }

        public void ResetAll()
        {
            Health?.Reset();
            Energy?.Reset();
            Combat?.Reset();
            Movement?.Reset();
            Mining?.Reset();
            Protect?.Reset();
        }
    }
}
