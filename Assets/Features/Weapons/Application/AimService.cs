using UnityEngine;
using Features.Weapons.Data;

namespace Features.Weapons.Application
{
    public class AimService
    {
        private WeaponConfig config;
        private bool aiming;

        public void Initialize(WeaponConfig config)
        {
            this.config = config;
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
            float spread = aiming ? config.adsSpread : config.hipfireSpread;

            Vector3 dir = camTransform.forward;

            float yaw   = Random.Range(-spread, spread);
            float pitch = Random.Range(-spread, spread);

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            return rot * dir;
        }
    }
}
