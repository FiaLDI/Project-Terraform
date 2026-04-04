using UnityEngine;
using FishNet;
using Features.Effects.Domain;
using Features.Effects.Application;
using Features.Buffs.Domain;

namespace CoreGameplay.Effects
{
    [RequireComponent(typeof(Collider))]
    public sealed class DamageZone : MonoBehaviour
    {
        [Header("Damage Settings")]
        public float damagePerTick = 10f;
        public float tickInterval = 1f;
        public DamageType damageType = DamageType.Generic;

        [Header("Layer Filtering")]
        public LayerMask damageLayers;

        private float timer;

        private EffectDefinition damageEffect;

        private void Awake()
        {
            damageEffect = new EffectDefinition
            {
                type = EffectType.DealDamage,
                targetMode = TargetMode.Self, // будет заменён вручную
                value = damagePerTick
            };
        }

        private void OnTriggerStay(Collider other)
        {
            if (!InstanceFinder.IsServer)
                return;

            if ((damageLayers.value & (1 << other.gameObject.layer)) == 0)
                return;

            if (!other.TryGetComponent<IBuffTarget>(out var target))
                return;

            timer -= Time.deltaTime;

            if (timer > 0f)
                return;

            timer = tickInterval;

            var ctx = new EffectContext(
                source: null,
                targets: new[] { target },
                origin: transform.position,
                direction: Vector3.zero
            );

            EffectExecutor.Instance.Execute(damageEffect, ctx);
        }

        private void OnTriggerExit(Collider other)
        {
            timer = 0f;
        }
    }
}
