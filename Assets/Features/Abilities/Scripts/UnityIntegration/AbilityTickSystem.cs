using UnityEngine;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Managing.Timing;
using Features.Abilities.Application;

namespace Features.Abilities.UnityIntegration
{
    [DefaultExecutionOrder(-1000)]
    public sealed class AbilityTickSystem : NetworkBehaviour
    {
        private static AbilityTickSystem instance;
        public static AbilityTickSystem Instance => instance;

        private readonly HashSet<AbilityCaster> casters = new();
        private static readonly HashSet<AbilityCaster> pending = new();

        // =====================================================
        // LIFECYCLE
        // =====================================================

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (instance != null && instance != this)
            {
                Debug.LogError("[AbilityTickSystem] Multiple instances!", this);
                return;
            }

            instance = this;

            foreach (var c in pending)
                if (c != null)
                    casters.Add(c);

            pending.Clear();

            TimeManager.OnTick += OnServerTick;

            Debug.Log(
                $"[AbilityTickSystem] SERVER started | Casters={casters.Count}",
                this
            );
        }

        public override void OnStopServer()
        {
            TimeManager.OnTick -= OnServerTick;

            casters.Clear();
            pending.Clear();

            if (instance == this)
                instance = null;

            base.OnStopServer();
        }

        // =====================================================
        // REGISTRATION
        // =====================================================

        public static void Register(AbilityCaster caster)
        {
            if (caster == null)
                return;

            if (Instance == null)
            {
                pending.Add(caster);
                return;
            }

            Instance.casters.Add(caster);
        }

        public static void Unregister(AbilityCaster caster)
        {
            if (caster == null)
                return;

            if (Instance != null)
                Instance.casters.Remove(caster);

            pending.Remove(caster);
        }

        // =====================================================
        // SERVER TICK
        // =====================================================

        private void OnServerTick()
        {
            if (!IsServerStarted)
                return;

            float dt = (float)TimeManager.TickDelta;

            foreach (var caster in casters)
            {
                if (caster != null && caster.IsReady)
                    caster.ServerTick(dt);
            }
        }
    }
}