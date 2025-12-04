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
            // fallback: use PlayerRegistry if owner lost
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

        // ============================================================
        // FOLLOW OWNER
        // ============================================================
        private void FollowOwner()
        {
            Vector3 target = owner.transform.position + new Vector3(2f, 3f, 0);
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * speed);
        }

        // ============================================================
        // HEAL AURA
        // ============================================================
        private void UpdateHealAura()
        {
            if (!healBuff) return;

            // NEW: wider search — not only Player layer
            Collider[] hits = Physics.OverlapSphere(transform.position, healRadius);

            HashSet<IBuffTarget> inside = new();

            foreach (var h in hits)
            {
                if (!h.TryGetComponent<IBuffTarget>(out var target))
                    continue;

                inside.Add(target);

                // NEW: Don't heal enemies or self unless desired
                // If needed — uncomment:
                // if (target.GameObject == this.gameObject) continue;

                // Already buffed?
                if (!active.ContainsKey(target))
                {
                    var inst = target.BuffSystem.Add(healBuff);
                    if (inst != null)
                        active[target] = inst;
                }
            }

            // remove buffs from those who left radius
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
            // cleanup
            foreach (var kv in active)
                kv.Key.BuffSystem?.Remove(kv.Value);

            active.Clear();
        }
    }
}
