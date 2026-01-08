using UnityEngine;

namespace Features.Stats.Net
{
    /// <summary>
    /// Гарантирует, что max-значение применяется
    /// ТОЛЬКО при реальном изменении.
    /// </summary>
    public sealed class StatApplyGuard
    {
        private float appliedMax = float.NaN;

        /// <summary>
        /// Возвращает true, если max нужно применить.
        /// </summary>
        public bool ShouldApply(float newMax)
        {
            if (float.IsNaN(appliedMax))
            {
                appliedMax = newMax;
                return true;
            }

            if (!Mathf.Approximately(appliedMax, newMax))
            {
                appliedMax = newMax;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Последнее применённое значение max
        /// </summary>
        public float Current => appliedMax;

        public void Reset()
        {
            appliedMax = float.NaN;
        }
    }
}
