using UnityEngine;

namespace Features.Stats.Net
{
    public enum StatsNetMode
    {
        None,
        HealthOnly,
        Full
    }

    [CreateAssetMenu(menuName = "Game/Stats/Net Profile")]
    public class StatsNetProfileSO : ScriptableObject
    {
        public StatsNetMode mode = StatsNetMode.Full;
    }
}
