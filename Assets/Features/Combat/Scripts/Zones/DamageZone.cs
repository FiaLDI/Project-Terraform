using UnityEngine;
using Features.Combat.Domain;
using Features.Combat.Application;

namespace Features.Combat.Zones
{
    public class DamageZone : MonoBehaviour
    {
        [Header("Damage Settings")]
        public float damagePerTick = 10f;
        public float tickInterval = 1f;
        public DamageType damageType = DamageType.Generic;

        [Header("Layer Filtering")]
        public LayerMask damageLayers; // <-- Вот он!

        private float timer;

        private void OnTriggerStay(Collider other)
        {
            // проверяем слой
            if ((damageLayers.value & (1 << other.gameObject.layer)) == 0)
                return; // слой не подходит → выходим

            // пробуем получить IDamageable
            if (!other.TryGetComponent<IDamageable>(out var target))
                return;

            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                float dmg = damagePerTick;

                // ShieldGrid & global modifiers
                dmg = DamageSystem.ApplyDamageModifiers(target, dmg, damageType);

                // нанести урон
                target.TakeDamage(dmg, damageType);

                timer = tickInterval;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            timer = 0f;
        }
    }
}
