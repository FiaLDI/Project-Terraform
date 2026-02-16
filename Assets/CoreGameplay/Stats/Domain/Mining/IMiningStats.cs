namespace Features.Stats.Domain
{
    public interface IMiningStats
    {
        float MiningPower { get; }
        void ApplyBase(float power);
        void Reset();
    }
}
