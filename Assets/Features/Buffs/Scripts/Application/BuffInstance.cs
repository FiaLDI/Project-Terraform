using UnityEngine;
using Features.Buffs.Domain;

namespace Features.Buffs.Application
{
    public class BuffInstance
    {
        public BuffSO Config { get; }
        public IBuffTarget Target { get; }

        public int StackCount { get; set; } = 1;

        /// <summary> Время, когда бафф должен исчезнуть </summary>
        public float EndTime { get; private set; }

        /// <summary> Сколько осталось секунд </summary>
        public float Remaining => Mathf.Max(0, EndTime - Time.time);

        /// <summary> true → если бафф истёк </summary>
        public bool IsExpired => Time.time >= EndTime;

        /// <summary> Прогресс 0..1 для UI </summary>
        public float Progress01 =>
            Config.duration <= 0 ? 0 : Mathf.Clamp01(Remaining / Config.duration);

        public BuffInstance(BuffSO cfg, IBuffTarget target)
        {
            Config = cfg;
            Target = target;
            EndTime = Time.time + cfg.duration;
        }

        public void Refresh()
        {
            EndTime = Time.time + Config.duration;
        }
    }
}
