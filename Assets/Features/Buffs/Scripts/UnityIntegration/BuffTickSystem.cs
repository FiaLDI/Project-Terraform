using UnityEngine;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Managing.Timing;
using Features.Buffs.Application;
using System.Linq;

namespace Features.Buffs.UnityIntegration
{
    [DefaultExecutionOrder(-1000)]
    public sealed class BuffTickSystem : NetworkBehaviour
    {
        // ================= SINGLETON =================

        private static BuffTickSystem _instance;
        public static BuffTickSystem Instance => _instance;

        // ================= ACTIVE =================

        private readonly HashSet<BuffSystem> buffSystems = new();
        private readonly HashSet<AreaBuffEmitter> emitters = new();

        // ================= PENDING =================

        private static readonly HashSet<BuffSystem> pendingSystems = new();
        private static readonly HashSet<AreaBuffEmitter> pendingEmitters = new();

        // =====================================================
        // LIFECYCLE
        // =====================================================

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (_instance != null && _instance != this)
            {
                Debug.LogError(
                    "[BuffTickSystem] Multiple instances detected!",
                    this
                );
                return;
            }

            _instance = this;

            // 🔥 принять отложенные регистрации (ТОЛЬКО серверные)
            foreach (var system in pendingSystems)
            {
                if (system != null && system.IsServerStarted)
                    buffSystems.Add(system);
            }

            foreach (var emitter in pendingEmitters)
            {
                if (emitter != null && emitter.IsServerStarted)
                    emitters.Add(emitter);
            }

            pendingSystems.Clear();
            pendingEmitters.Clear();

            TimeManager.OnTick += OnServerTick;

            Debug.Log(
                $"[BuffTickSystem] SERVER started | BuffSystems={buffSystems.Count} Emitters={emitters.Count}",
                this
            );
        }

        public override void OnStopServer()
        {
            TimeManager.OnTick -= OnServerTick;

            buffSystems.Clear();
            emitters.Clear();

            pendingSystems.Clear();
            pendingEmitters.Clear();

            if (_instance == this)
                _instance = null;

            base.OnStopServer();
        }

        // =====================================================
        // REGISTRATION API (SERVER ONLY)
        // =====================================================

        public static void Register(BuffSystem system)
        {
            if (system == null || !system.IsServerStarted)
                return;

            if (Instance == null)
            {
                pendingSystems.Add(system);
                return;
            }

            Instance.buffSystems.Add(system);
        }

        public static void Unregister(BuffSystem system)
        {
            if (system == null)
                return;

            if (Instance != null)
                Instance.buffSystems.Remove(system);

            pendingSystems.Remove(system);
        }

        public static void RegisterEmitter(AreaBuffEmitter emitter)
        {
            if (emitter == null || !emitter.IsServerStarted)
                return;

            if (Instance == null)
            {
                pendingEmitters.Add(emitter);
                return;
            }

            Instance.emitters.Add(emitter);
        }

        public static void UnregisterEmitter(AreaBuffEmitter emitter)
        {
            if (emitter == null)
                return;

            if (Instance != null)
                Instance.emitters.Remove(emitter);

            pendingEmitters.Remove(emitter);
        }

        // =====================================================
        // SERVER TICK
        // =====================================================

        private void OnServerTick()
        {
            if (!IsServerStarted)
                return;

            float dt = (float)TimeManager.TickDelta;

            // Snapshot — безопасно при unregister во время тика
            foreach (var system in buffSystems.ToArray())
            {
                if (system != null && system.ServiceReady)
                    system.Tick(dt);
            }

            foreach (var emitter in emitters.ToArray())
            {
                if (emitter != null)
                    emitter.Tick();
            }
        }
    }
}
