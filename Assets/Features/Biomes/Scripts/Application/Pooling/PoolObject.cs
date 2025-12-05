using UnityEngine;

namespace Features.Pooling
{
    /// <summary>
    /// Компонент, который делает объект управляемым пулом.
    /// </summary>
    public class PoolObject : MonoBehaviour
    {
        /// <summary>
        /// Пул, из которого объект был получен.
        /// </summary>
        public SmartPool Pool { get; set; }

        /// <summary>
        /// Метаданные, в т.ч. prefabIndex.
        /// </summary>
        public PoolMeta meta;

        /// <summary>
        /// Вернуть объект в пул или уничтожить, если пул не задан.
        /// Вызывать только игровым кодом (не самим пулом).
        /// </summary>
        public void ReturnToPool()
        {
            if (Pool != null)
            {
                Pool.Release(this);
            }
            else
            {
                // На всякий случай — если объект был создан обычным Instantiate
                Object.Destroy(gameObject);
            }
        }
    }
}
