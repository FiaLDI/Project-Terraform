using UnityEngine;

namespace Features.Pooling
{
    /// <summary>
    /// Доп.данные для объектов пула.
    /// Главное поле — prefabIndex, по нему SmartPool понимает,
    /// в какой стек возвращать объект.
    /// </summary>
    public class PoolMeta : MonoBehaviour
    {
        [Tooltip("Index префаба в реестре InstanceRegistry")]
        public int prefabIndex;
    }
}
