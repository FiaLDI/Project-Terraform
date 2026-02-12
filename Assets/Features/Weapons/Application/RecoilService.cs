using Features.Stats.Domain;
using Features.Weapons.Domain;
using UnityEngine;

namespace Features.Weapons.Application
{
    public class RecoilService
    {
        ICombatStats stats;
        private AnimationCurve pattern;
        private int shotIndex;

        public void Initialize(
            ICombatStats stats,
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
            float vertical = stats.Recoil;
            float horizontal = Random.Range(
                -stats.Recoil,
                stats.Recoil
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
