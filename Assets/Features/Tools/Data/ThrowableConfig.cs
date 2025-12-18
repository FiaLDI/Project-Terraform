using UnityEngine;

namespace Features.Tools.Data
{
    [CreateAssetMenu(menuName = "Configs/Throwable")]
    public class ThrowableConfig : ScriptableObject
    {
        [Header("Projectile prefab")]
        public GameObject projectilePrefab;

        [Header("Stats")]
        public float baseThrowForce = 15f;
    }
}
