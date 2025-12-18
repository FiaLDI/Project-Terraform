using UnityEngine;
using Features.Weapons.Domain;

namespace Features.Weapons.Application
{
    public class RecoilService
    {
        private WeaponRuntimeStats stats;
        private AnimationCurve pattern;
        private int shotIndex;

        public void Initialize(
            WeaponRuntimeStats stats,
            AnimationCurve recoilPattern = null)
        {
            this.stats = stats;
            this.pattern = recoilPattern;
            shotIndex = 0;
        }

        /// <summary>
        /// Возвращает смещение отдачи
        /// </summary>
        public Vector2 GetRecoil()
        {
            float vertical = stats.recoil;
            float horizontal = Random.Range(
                -stats.recoil,
                stats.recoil
            );

            if (pattern != null && pattern.length > 0)
            {
                vertical *= pattern.Evaluate(shotIndex);
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
