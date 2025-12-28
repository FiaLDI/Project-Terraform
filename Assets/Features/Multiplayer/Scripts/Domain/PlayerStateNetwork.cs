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



        public override void OnStartClient()
        {
            base.OnStartClient();

            Debug.Log(
                $"[PlayerStateNetwork] OnStartClient - " +
                $"_visualId='{_visualId.Value}', visualController={visualController != null}", 
                this
            );

            if (!string.IsNullOrEmpty(_visualId.Value) && visualController != null)
            {
                Debug.Log($"[PlayerStateNetwork] OnStartClient applying visual: {_visualId.Value}", this);
                visualController.ApplyVisual(_visualId.Value);
            }
        }



        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            // 🟢 Инициализируем компоненты
            classController = GetComponent<PlayerClassController>();
            visualController = GetComponent<PlayerVisualController>();
            netAdapter = GetComponent<PlayerStateNetAdapter>();

            if (classController == null)
                Debug.LogError("[PlayerStateNetwork] PlayerClassController not found!", this);
            if (visualController == null)
                Debug.LogError("[PlayerStateNetwork] PlayerVisualController not found!", this);
            if (netAdapter == null)
                Debug.LogError("[PlayerStateNetwork] PlayerStateNetAdapter not found!", this);

            Debug.Log($"[PlayerStateNetwork] OnStartNetwork initialized", this);

            // 🟢 Подписываемся на изменения SyncVar
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

            Debug.Log($"[PlayerStateNetwork] OnStartServer", this);

            // Компоненты уже инициализированы в OnStartNetwork
            if (classController == null)
            {
                Debug.LogError("[PlayerStateNetwork] classController is NULL!", this);
                return;
            }

            // 🟢 Устанавливаем начальный класс на сервере
            string defaultClass = classController.CurrentClassId;
            Debug.Log($"[PlayerStateNetwork] OnStartServer - Setting default class: {defaultClass}", this);
            
            SetClass(defaultClass);
        }



        /// <summary>
        /// 🟢 Единственная серверная точка для смены класса
        /// Синхронизирует класс и визуал через SyncVar
        /// Вызывает RPC через PlayerStateNetAdapter
        /// </summary>
        [Server]
        public void SetClass(string classId)
        {
            if (string.IsNullOrEmpty(classId))
            {
                Debug.LogWarning("[PlayerStateNetwork] SetClass called with empty classId", this);
                return;
            }

            if (_classId.Value == classId)
            {
                Debug.Log($"[PlayerStateNetwork] Class already set to {classId}, skipping", this);
                return;
            }

            Debug.Log($"[PlayerStateNetwork-Server] SetClass({classId})", this);

            // 🟢 Обновляем SyncVar класса (это вызовет OnClassChanged на клиентах)
            _classId.Value = classId;

            // 🟢 Обновляем визуал (зависит от класса)
            if (classController != null)
            {
                string visualId = classController.CurrentVisualId;
                _visualId.Value = visualId;
                Debug.Log($"[PlayerStateNetwork-Server] Updated visual: {visualId}", this);
            }

            // 🟢 Вызываем PlayerStateNetAdapter для загрузки абилити через RPC
            if (netAdapter != null)
            {
                Debug.Log($"[PlayerStateNetwork-Server] Calling netAdapter.ApplyClass({classId})", this);
                netAdapter.ApplyClass(classId);
            }
            else
            {
                Debug.LogError("[PlayerStateNetwork] PlayerStateNetAdapter is null!", this);
            }
        }



        /* ================= CLIENT ================= */



        /// <summary>
        /// Вызывается когда SyncVar _classId изменяется
        /// На клиенте: логирует событие (реальная работа в PlayerStateNetAdapter RPC)
        /// На сервере: ничего не делает (уже обработано)
        /// </summary>
        private void OnClassChanged(string oldValue, string newValue, bool asServer)
        {
            Debug.Log(
                $"[PlayerStateNetwork] OnClassChanged: {oldValue} → {newValue}, asServer={asServer}", 
                this
            );

            if (asServer)
            {
                // На сервере SyncVar уже обновлён в SetClass(), ничего не делаем
                return;
            }

            if (string.IsNullOrEmpty(newValue))
            {
                Debug.LogWarning("[PlayerStateNetwork] OnClassChanged called with empty value", this);
                return;
            }

            // На клиенте: RPC уже отправлен через PlayerStateNetAdapter
            // Данные о статах и абилити синхронизируются через RpcApplyClassWithAbilities
            Debug.Log($"[PlayerStateNetwork-Client] Class notification received: {newValue}", this);
        }



        /// <summary>
        /// Вызывается когда SyncVar _visualId изменяется
        /// Применяет визуал на всех клиентах когда синхронизируется
        /// </summary>
        private void OnVisualChanged(string oldValue, string newValue, bool asServer)
        {
            Debug.Log(
                $"[PlayerStateNetwork] OnVisualChanged: '{oldValue}' → '{newValue}', " +
                $"asServer={asServer}, visualController={visualController != null}", 
                this
            );

            if (string.IsNullOrEmpty(newValue))
                return;

            // ✅ Применяем визуал ВСЕГДА когда значение изменилось
            if (visualController != null)
            {
                Debug.Log($"[PlayerStateNetwork] Applying visual: {newValue}", this);
                visualController.ApplyVisual(newValue);
            }
            else
            {
                Debug.LogError("[PlayerStateNetwork] visualController is NULL!", this);
            }
        }
    }
}
