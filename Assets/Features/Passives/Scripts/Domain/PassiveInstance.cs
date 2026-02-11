namespace Features.Passives.Domain
{
    public sealed class PassiveInstance
    {
        public PassiveSO Config { get; }

        public PassiveInstance(PassiveSO config)
        {
            Config = config;
        }
    }
}
