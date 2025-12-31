using UnityEngine;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Managing.Timing;
using Features.Buffs.Application;
using Features.Buffs.UnityIntegration;

namespace Features.Buffs.UnityIntegration
{
    /// <summary>
    /// Глобальный серверный тикер баффов.
    /// В СЦЕНЕ ДОЛЖЕН БЫТЬ РОВНО 1 ЭКЗЕМПЛЯР.
    ///
    /// Отвечает за:
    /// - тик всех BuffSystem
    /// - тик всех AreaBuffEmitter
    ///
    /// Работает ТОЛЬКО на сервере.
    /// </summary>
    public sealed class BuffTickSystem : NetworkBehaviour
    {
        private static BuffTickSystem _instance;
        public static BuffTickSystem Instance => _instance;

        // Все BuffSystem (игроки, мобы, турели и т.д.)
        private readonly HashSet<BuffSystem> buffSystems = new();

        // Все AreaBuffEmitter (ауры)
        private readonly HashSet<AreaBuffEmitter> emitters = new();

        // =====================================================
        // LIFECYCLE
        // =====================================================

        public override void OnStartServer()
        {
            base.OnStartServer();

            if (_instance != null)
            {
                Debug.LogError("[BuffTickSystem] Multiple instances detected! There must be ONLY ONE.", this);
                return;
            }

            _instance = this;

            TimeManager.OnTick += OnServerTick;

            Debug.Log("[BuffTickSystem] SERVER started", this);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            TimeManager.OnTick -= OnServerTick;

            if (_instance == this)
                _instance = null;
        }

        // =====================================================
        // REGISTRATION API
        // =====================================================

        public static void Register(BuffSystem system)
        {
            if (system == null)
                return;

            if (Instance == null || !Instance.IsServerStarted)
                return;

            Instance.buffSystems.Add(system);
        }

        public static void Unregister(BuffSystem system)
        {
            if (Instance == null || system == null)
                return;

            Instance.buffSystems.Remove(system);
        }

        public static void RegisterEmitter(AreaBuffEmitter emitter)
        {
            if (emitter == null)
                return;

            if (Instance == null || !Instance.IsServerStarted)
                return;

            Instance.emitters.Add(emitter);
        }

        public static void UnregisterEmitter(AreaBuffEmitter emitter)
        {
            if (Instance == null || emitter == null)
                return;

            Instance.emitters.Remove(emitter);
        }

        // =====================================================
        // SERVER TICK
        // =====================================================

        private void OnServerTick()
        {
            if (!IsServerStarted)
                return;

            float dt = (float)TimeManager.TickDelta;

            // 1️⃣ Тикаем все BuffSystem (длительности, DoT, HoT и т.п.)
            foreach (var system in buffSystems)
            {
                if (system != null && system.ServiceReady)
                    system.Tick(dt);
            }

            // 2️⃣ Тикаем все AreaBuffEmitter (вход / выход из аур)
            foreach (var emitter in emitters)
            {
                if (emitter != null)
                    emitter.Tick();
            }
        }
    }
}
