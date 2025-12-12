using System.Collections.Generic;
using UnityEngine;

namespace Features.Biomes.Utility
{
    public static class Vector3Extensions
    {
        public static bool IsFiniteSafe(this Vector3 v)
        {
            return v.x.IsFiniteSafe() && v.y.IsFiniteSafe() && v.z.IsFiniteSafe();
        }
    }
}
