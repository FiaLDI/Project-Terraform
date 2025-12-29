using Features.Class.Net;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System.Collections;

namespace Features.Player.UnityIntegration
{
    [RequireComponent(typeof(PlayerClassController))]
    [RequireComponent(typeof(PlayerVisualController))]
    [RequireComponent(typeof(PlayerStateNetAdapter))]
    public sealed class PlayerStateNetwork : NetworkBehaviour
    {
        // Обязательно Hook, чтобы ловить изменения в реальном времени
        private readonly SyncVar<string> _classId = new();

        // Hook для визуала
        private readonly SyncVar<string> _visualId = new();

        private PlayerClassController classController;
        private PlayerVisualController visualController;
        private PlayerStateNetAdapter netAdapter;

        private void Awake() => InitializeComponents();

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            InitializeComponents();
            _classId.OnChange += OnClassChanged;
            _visualId.OnChange += OnVisualChanged;
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            _classId.OnChange -= OnClassChanged;
            _visualId.OnChange -= OnVisualChanged;
        }

        // === CLIENT: LATE JOIN FIX ===
        public override void OnStartClient()
        {
            base.OnStartClient();

            // 1. Subscribe to SyncVar changes for future updates
            _visualId.OnChange += OnVisualIdChanged;

            // 2. LATE JOIN CHECK:
            // If we join late, the SyncVar is already populated by the server.
            // We must apply it immediately because the OnChange event won't fire for the initial value.
            if (!string.IsNullOrEmpty(_visualId.Value))
            {
                Debug.Log($"[PlayerStateNetwork] Late Join: Found existing Visual ID '{_visualId.Value}'. Applying now.");
                ApplyVisual(_visualId.Value);
            }

            if (!string.IsNullOrEmpty(_classId.Value))
            {
                Debug.Log($"[PlayerStateNetwork] Late Join: Found existing Class ID '{_classId.Value}'.");
                // Optional: Ensure class is applied if the RPC missed it, 
                // though the RPC usually handles this well.
            }
        }

        private void ApplyVisual(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            // Forward to your visual controller
            GetComponent<PlayerVisualController>().ApplyVisual(id);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            _visualId.OnChange -= OnVisualIdChanged;
        }

        private void OnVisualIdChanged(string prev, string next, bool asServer)
        {
            if (asServer) return; // Client only
            ApplyVisual(next);
        }

        private IEnumerator ApplyDataWithDelay()
        {
            // Ждем конца кадра, чтобы все Awake/Start отработали
            yield return new WaitForEndOfFrame();

            // Если это Late Join (мы подключились, а переменные уже заполнены сервером)
            if (!string.IsNullOrEmpty(_visualId.Value))
            {
                Debug.Log($"[PlayerStateNetwork] LateJoin: Force Applying Visual '{_visualId.Value}'", this);
                if (visualController != null)
                    visualController.ApplyVisual(_visualId.Value);
            }
        }

        // === SERVER ===
        public override void OnStartServer()
        {
            base.OnStartServer();
            if (classController == null) return;

            // Логика подбора дефолтного класса
            string cid = classController.CurrentClassId;
            string vid = string.Empty;

            var cls = classController.GetCurrentClass();
            if (cls != null)
            {
                cid = cls.id;
                if (cls.visualPreset != null) vid = cls.visualPreset.id;
            }

            if (!string.IsNullOrEmpty(cid))
            {
                SetClass(cid, vid);
            }
        }

        [Server]
        public void SetClass(string classId, string visualId)
        {
            if (string.IsNullOrEmpty(classId)) return;

            // Обновляем данные сети
            _classId.Value = classId;
            _visualId.Value = visualId;

            // 1. Применяем логику класса через адаптер (он там почистит баффы)
            if (netAdapter != null)
                netAdapter.ApplyClass(classId);

            // 2. Применяем визуал на сервере (хосте) вручную, т.к. SyncVar Hook не срабатывает для сервера
            if (visualController != null && !string.IsNullOrEmpty(visualId))
                visualController.ApplyVisual(visualId);
        }

        // === HOOKS ===
        private void OnClassChanged(string oldVal, string newVal, bool asServer)
        {
            if (asServer) return;
            // Логика UI если нужна
        }

        private void OnVisualChanged(string oldVal, string newVal, bool asServer)
        {
            if (asServer) return; // Сервер сам применяет в SetClass

            if (!string.IsNullOrEmpty(newVal) && visualController != null)
            {
                visualController.ApplyVisual(newVal);
            }
        }

        private void InitializeComponents()
        {
            if (classController == null) classController = GetComponent<PlayerClassController>();
            if (visualController == null) visualController = GetComponent<PlayerVisualController>();
            if (netAdapter == null) netAdapter = GetComponent<PlayerStateNetAdapter>();
        }
    }
}
