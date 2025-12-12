using UnityEngine;
using Features.Weapons.Data;

namespace Features.Weapons.Application
{
    public class RecoilService
    {
        private WeaponConfig config;
        private int shotIndex;

        public void Initialize(WeaponConfig config)
        {
            this.config = config;
            shotIndex = 0;
        }

        /// <summary>
        /// Возвращает смещение отдачи
        /// </summary>
        public Vector2 GetRecoil()
        {
            float vertical = config.verticalRecoil;
            float horizontal = Random.Range(
                -config.horizontalRecoil,
                config.horizontalRecoil
            );

            if (config.recoilPattern != null && config.recoilPattern.length > 0)
            {
                vertical *= config.recoilPattern.Evaluate(shotIndex);
            }

            shotIndex++;
            return new Vector2(horizontal, vertical);
        }

        public void Reset()
        {
            shotIndex = 0;
        }
    }
}
