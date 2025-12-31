using Features.Class.Net;
using Features.Classes.Data;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    /// <summary>
    /// Сетевое состояние игрока.
    ///
    /// ОТВЕТСТВЕННОСТЬ:
    /// - хранит SyncVar состояния (classId, visualId)
    /// - инициирует серверное применение класса
    /// - применяет визуал на клиентах
    ///
    /// НЕ:
    /// - считает статы
    /// - применяет бафы
    /// - управляет способностями
    /// </summary>
    public sealed class PlayerStateNetwork : NetworkBehaviour
    {
        // =====================================================
        // NETWORK STATE
        // =====================================================

        private readonly SyncVar<string> _classId = new();
        private readonly SyncVar<string> _visualId = new();

        // =====================================================
        // COMPONENTS
        // =====================================================

        private PlayerVisualController visualController;
        private PlayerStateNetAdapter netAdapter;

        // =====================================================
        // DATA
        // =====================================================

        private PlayerClassLibrarySO classLibrary;
        private string _preInitClass;

        // =====================================================
        // LIFECYCLE
        // =====================================================

        private void Awake()
        {
            visualController = GetComponent<PlayerVisualController>();
            netAdapter = GetComponent<PlayerStateNetAdapter>();

            classLibrary = UnityEngine.Resources.Load<PlayerClassLibrarySO>(
                "Databases/PlayerClassLibrary");

            if (classLibrary == null)
            {
                Debug.LogError(
                    "[PlayerStateNetwork] PlayerClassLibrary not found in Resources/Databases",
                    this
                );
            }
        }

        /// <summary>
        /// Вызывается сервером ДО Spawn (из NetworkPlayerService).
        /// </summary>
        public void PreInitClass(string classId)
        {
            _preInitClass = classId;
        }

        // =====================================================
        // SERVER
        // =====================================================

        public override void OnSpawnServer(NetworkConnection conn)
        {
            base.OnSpawnServer(conn);

            // 1️⃣ Определяем класс (гарантированно)
            string classId = string.IsNullOrEmpty(_preInitClass)
                ? "0" // дефолтный класс
                : _preInitClass;

            var cls = classLibrary.FindById(classId);
            if (cls == null)
            {
                Debug.LogError(
                    $"[PlayerStateNetwork] Class '{classId}' not found in library",
                    this
                );
                return;
            }

            // 2️⃣ Записываем сетевое состояние
            _classId.Value = classId;
            _visualId.Value = cls.visualPreset.id;

            // 3️⃣ ИНИЦИИРУЕМ серверный pipeline
            netAdapter.ApplyClassWhenReady(classId);

            Debug.Log(
                $"[PlayerStateNetwork] Spawned with class='{classId}' visual='{_visualId.Value}'",
                this
            );
        }

        // =====================================================
        // NETWORK EVENTS
        // =====================================================

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            _visualId.OnChange += OnVisualChanged;
        }

        public override void OnStopNetwork()
        {
            _visualId.OnChange -= OnVisualChanged;
        }

        // =====================================================
        // VISUAL (CLIENT + HOST)
        // =====================================================

        private void OnVisualChanged(string oldVal, string newVal, bool _)
        {
            if (string.IsNullOrEmpty(newVal))
                return;

            visualController.ApplyVisual(newVal);
        }

        // =====================================================
        // SERVER API
        // =====================================================

        /// <summary>
        /// Серверная смена класса в рантайме.
        /// </summary>
        [Server]
        public void SetClass(string classId)
        {
            var cls = classLibrary.FindById(classId);
            if (cls == null)
                return;

            _classId.Value = classId;
            _visualId.Value = cls.visualPreset.id;

            netAdapter.ApplyClassWhenReady(classId);
        }
    }
}
