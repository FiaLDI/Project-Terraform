using Features.Stats.Domain;
using Features.Weapons.Domain;
using UnityEngine;

namespace Features.Weapons.Application
{
    public class AimService
    {
        private ICombatStats stats;
        private bool aiming;

        public bool IsAiming => aiming;

        public void Initialize(ICombatStats stats)
        {
            this.stats = stats;
        }

        public void SetAiming(bool value)
        {
            aiming = value;
        }

        /// <summary>
        /// Возвращает направление с учётом spread
        /// </summary>
        public Vector3 GetSpreadDirection(Transform camTransform)
        {
            float spread = aiming ? stats.AimSpread : stats.Spread;

            Vector3 dir = camTransform.forward;

            float yaw   = Random.Range(-spread, spread);
            float pitch = Random.Range(-spread, spread);

            return Quaternion.Euler(pitch, yaw, 0f) * dir;
        }
    }
}
