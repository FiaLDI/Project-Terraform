using System.Collections.Generic;
using UnityEngine;

namespace Features.Biomes.Utility
{
    public static class FloatExtensions
    {
        public static bool IsFiniteSafe(this float f)
        {
            return !(float.IsNaN(f) || float.IsInfinity(f));
        }
    }
}
