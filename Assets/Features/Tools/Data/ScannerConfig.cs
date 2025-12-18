using UnityEngine;

namespace Features.Tools.Data
{
    [CreateAssetMenu(menuName = "Configs/Scanner")]
    public class ScannerConfig : ScriptableObject
    {
        public float baseScanRange = 10f;
        public float baseScanSpeed = 1f;
        public float baseCooldown = 0.6f;
    }
}
