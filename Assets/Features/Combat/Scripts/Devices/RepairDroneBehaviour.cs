using UnityEngine;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Buffs.Application;

namespace Features.Combat.Devices
{
    public class RepairDroneBehaviour : MonoBehaviour
    {
        private GameObject owner;
        private float lifetime;
        private float speed;

        [Header("Healing Aura")]
        public BuffSO healBuff; 
        public float healRadius = 6f;

        private Dictionary<IBuffTarget, BuffInstance> active = new();
        private float elapsed;

        public void Init(GameObject owner, float lifetime, float speed)
        {
            this.owner = owner;
            this.lifetime = lifetime;
            this.speed = speed;
        }

        private void Update()
        {
            if (!owner) return;

            // follow owner smoothly
            Vector3 target = owner.transform.position + new Vector3(2f, 3f, 0);
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * speed);

            UpdateHealAura();

            elapsed += Time.deltaTime;
            if (elapsed >= lifetime)
                Destroy(gameObject);
        }

        // ============================================
        // HEAL AURA
        // ============================================
        private void UpdateHealAura()
        {
            if (!healBuff) return;

            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                healRadius,
                LayerMask.GetMask("Player")  // или все цели → default
            );

            HashSet<IBuffTarget> inside = new();

            foreach (var h in hits)
            {
                if (!h.TryGetComponent<IBuffTarget>(out var target))
                    continue;

                inside.Add(target);

                // Already active?
                if (!active.ContainsKey(target))
                {
                    var inst = target.BuffSystem.Add(healBuff);
                    if (inst != null)
                        active[target] = inst;
                }
            }

            // remove buff when target leaves zone
            List<IBuffTarget> toRemove = new();

            foreach (var kv in active)
            {
                if (!inside.Contains(kv.Key))
                    toRemove.Add(kv.Key);
            }

            foreach (var t in toRemove)
            {
                t.BuffSystem.Remove(active[t]);
                active.Remove(t);
            }
        }

        private void OnDestroy()
        {
            // cleanup buffs
            foreach (var kv in active)
                kv.Key.BuffSystem?.Remove(kv.Value);

            active.Clear();
        }
    }
}
