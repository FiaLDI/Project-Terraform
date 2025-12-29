using Features.Class.Net;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    [RequireComponent(typeof(PlayerClassController))]
    [RequireComponent(typeof(PlayerVisualController))]
    [RequireComponent(typeof(PlayerStateNetAdapter))]
    public sealed class PlayerStateNetwork : NetworkBehaviour
    {
        private readonly SyncVar<string> _classId = new();
        private readonly SyncVar<string> _visualId = new();

        private PlayerClassController classController;
        private PlayerVisualController visualController;
        private PlayerStateNetAdapter netAdapter;

        /* ================= LIFECYCLE ================= */

        private void Awake()
        {
            InitializeComponents();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // Страховка инициализации
            if (visualController == null) InitializeComponents();

            Debug.Log($"[PlayerStateNetwork] OnStartClient - _visualId='{_visualId.Value}'", this);

            // ФИКС 1: Принудительно применяем визуал при входе (Late Join)
            if (!string.IsNullOrEmpty(_visualId.Value))
            {
                OnVisualChanged(string.Empty, _visualId.Value, false);
            }
            else
            {
                // Для отладки: если тут пусто для ЧУЖОГО игрока — значит сервер не записал данные
                if (!IsOwner) Debug.LogWarning("[PlayerStateNetwork] OnStartClient: _visualId is empty! Visual will not appear.", this);
            }
        }

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

        /* ================= SERVER ================= */

        public override void OnStartServer()
        {
            base.OnStartServer();
            if (classController == null) return;

            // Инициализация дефолтного класса
            string defaultClass = classController.CurrentClassId;
            SetClass(defaultClass);
        }

        [Server]
        public void SetClass(string classId)
        {
            if (string.IsNullOrEmpty(classId)) return;
            if (_classId.Value == classId) return;

            // ФИКС 2: Сначала применяем класс локально на сервере, чтобы currentClass не был null
            if (classController != null)
            {
                classController.ApplyClass(classId);
            }

            _classId.Value = classId;

            // Теперь CurrentVisualId вернет корректное значение
            if (classController != null)
            {
                _visualId.Value = classController.CurrentVisualId;
                Debug.Log($"[PlayerStateNetwork-Server] SetClass: Synced VisualID = {_visualId.Value}", this);
            }

            if (netAdapter != null)
            {
                netAdapter.ApplyClass(classId);
            }
        }

        /* ================= CLIENT LOGIC (OnClassChanged, OnVisualChanged) ================= */
        // Оставляем без изменений, как в вашем последнем варианте
        
        private void OnClassChanged(string oldValue, string newValue, bool asServer)
        {
            if (asServer) return;
            // Клиентская логика уведомления...
        }

        private void OnVisualChanged(string oldValue, string newValue, bool asServer)
        {
            if (string.IsNullOrEmpty(newValue)) return;
            if (visualController != null) visualController.ApplyVisual(newValue);
        }

        private void InitializeComponents()
        {
            if (classController == null) classController = GetComponent<PlayerClassController>();
            if (visualController == null) visualController = GetComponent<PlayerVisualController>();
            if (netAdapter == null) netAdapter = GetComponent<PlayerStateNetAdapter>();
        }
    }
}
