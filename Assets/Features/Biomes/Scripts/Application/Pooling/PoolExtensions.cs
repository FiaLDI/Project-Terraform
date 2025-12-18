using System.Collections.Generic;
using UnityEngine;

namespace Features.Pooling
{
    public static class PoolExtensions
    {
        public static int GetPrefabIndex(this PoolObject obj)
        {
            return obj.meta != null ? obj.meta.prefabIndex : -1;
        }
    }
}
