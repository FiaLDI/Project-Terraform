using UnityEngine;
using Features.Buffs.Domain;
using Features.Stats.Domain;

namespace Features.Buffs.Application
{
    public sealed class BuffExecutor : MonoBehaviour
    {
        public static BuffExecutor Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ================= APPLY =================

        public void Apply(BuffInstance inst)
        {
            var stats = inst.Target.GetServerStats();
            if (stats == null)
                return;

            foreach (var effect in inst.Config.effects)
                effect.ApplyWithContext(inst, stats);
        }

        // ================= TICK =================

        public void Tick(BuffInstance inst, float dt)
        {
            var stats = inst.Target.GetServerStats();
            if (stats == null)
                return;

            foreach (var effect in inst.Config.effects)
                effect?.Tick(stats, dt);
        }

        // ================= EXPIRE =================

        public void Expire(BuffInstance inst)
        {
            var stats = inst.Target.GetServerStats();
            if (stats == null)
                return;

            foreach (var effect in inst.Config.effects)
                effect?.Expire(stats);
        }
    }
}
