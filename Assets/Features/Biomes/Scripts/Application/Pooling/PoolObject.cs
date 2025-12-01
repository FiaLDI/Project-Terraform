using UnityEngine;

namespace Features.Pooling
{
    public class PoolObject : MonoBehaviour
    {
        public SmartPool Pool { get; set; }

        public PoolMeta meta;

        public void ReturnToPool()
        {
            // Если объект пришёл из пула – вернём его в пул
            if (Pool != null)
            {
                Pool.Release(this);
            }
            else
            {
                // Если пул не прописан (заспавнен обычным Instantiate),
                // просто уничтожаем объект, чтобы не ловить NullReference
                Object.Destroy(gameObject);
            }
        }
    }
}
