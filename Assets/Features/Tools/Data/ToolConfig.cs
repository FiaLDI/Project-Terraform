using UnityEngine;

namespace Features.Tools.Data
{
    [CreateAssetMenu(menuName = "Configs/Tool")]
    public class ToolConfig : ScriptableObject
    {
        public float baseMiningSpeed = 1f;
        public float baseDamage = 1f;
        public float baseRange = 3f;
    }
}
