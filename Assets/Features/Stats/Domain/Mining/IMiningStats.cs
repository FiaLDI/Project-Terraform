using Features.Buffs.Domain;

namespace Features.Stats.Domain
{
    public interface IMiningStats
    {
        float MiningPower { get; }

        void ApplyBase(float pwr);

        /// <summary>Бафф на майнинг (BuffStat.PlayerMiningSpeed).</summary>
        void ApplyBuff(BuffSO cfg, bool apply);
    }
}
