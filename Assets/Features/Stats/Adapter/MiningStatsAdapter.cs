using UnityEngine;
using Features.Stats.Domain;


namespace Features.Stats.Adapter
{
    public class MiningStatsAdapter : MonoBehaviour
    {
        private IMiningStats _stats;

        public float MiningPower => _stats.MiningPower;

        public void Init(IMiningStats stats)
        {
            _stats = stats;
        }

        // пример API
        public float GetMiningSpeedMultiplier()
        {
            return _stats.MiningPower;
        }
    }
}