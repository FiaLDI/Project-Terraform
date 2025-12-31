using UnityEngine;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Player.UnityIntegration;

namespace Features.Combat.Devices
{
    public sealed class RepairDroneBehaviour : MonoBehaviour, IBuffSource
    {
        private GameObject owner;
        private float lifetime;
        private float speed;

        [Header("Healing Aura")]
        public BuffSO healBuff;
        public float healRadius = 6f;

        private readonly HashSet<IBuffTarget> inside = new();
        private float elapsed;

        public void Init(GameObject owner, float lifetime, float speed)
        {
            this.owner = owner;
            this.lifetime = lifetime;
            this.speed = speed;
        }

        private void Update()
        {
            if (!owner)
                owner = PlayerRegistry.Instance.LocalPlayer;

            if (!owner)
                return;

            FollowOwner();
            UpdateHealAura();

            elapsed += Time.deltaTime;
            if (elapsed >= lifetime)
                Destroy(gameObject);
        }

        private void FollowOwner()
        {
            Vector3 target = owner.transform.position + new Vector3(2f, 3f, 0);
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * speed);
        }

        private void UpdateHealAura()
        {
            if (!healBuff) return;

            Collider[] hits = Physics.OverlapSphere(transform.position, healRadius);
            var current = new HashSet<IBuffTarget>();

            foreach (var h in hits)
            {
                if (!h.TryGetComponent(out IBuffTarget target))
                    continue;

                current.Add(target);

                if (!inside.Contains(target))
                {
                    target.BuffSystem?.Add(
                        healBuff,
                        source: this,
                        lifetimeMode: BuffLifetimeMode.WhileSourceAlive
                    );
                }
            }

            foreach (var target in inside)
            {
                if (!current.Contains(target))
                    target.BuffSystem?.RemoveBySource(this);
            }

            inside.Clear();
            inside.UnionWith(current);
        }

        private void OnDestroy()
        {
            foreach (var target in inside)
                target.BuffSystem?.RemoveBySource(this);

            inside.Clear();
        }
    }
}
