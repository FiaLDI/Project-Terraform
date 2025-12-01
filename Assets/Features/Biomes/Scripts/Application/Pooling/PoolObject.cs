using UnityEngine;

namespace Features.Pooling
{
    public class PoolObject : MonoBehaviour
    {
        public SmartPool Pool { get; set; }

        public PoolMeta meta;

        public void ReturnToPool()
        {
            Pool.Release(this);
        }
    }
}
