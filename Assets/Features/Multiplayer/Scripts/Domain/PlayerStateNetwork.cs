using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    [RequireComponent(typeof(PlayerClassController))]
    [RequireComponent(typeof(PlayerVisualController))]
    public sealed class PlayerStateNetwork : NetworkBehaviour
    {
        private readonly SyncVar<string> _classId = new();
        private readonly SyncVar<string> _visualId = new();

        private PlayerClassController classController;
        private PlayerVisualController visualController;

        /* ================= LIFECYCLE ================= */

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            classController = GetComponent<PlayerClassController>();
            visualController = GetComponent<PlayerVisualController>();

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

            if (classController == null)
            {
                Debug.LogError("[PlayerStateNetwork] PlayerClassController missing", this);
                return;
            }

            // Сервер инициализирует snapshot
            _classId.Value = classController.CurrentClassId;
            _visualId.Value = classController.CurrentVisualId;

            Debug.Log(
                $"[PlayerStateNetwork] Init state class={_classId.Value}, visual={_visualId.Value}",
                this
            );
        }

        /// <summary>
        /// Единственная серверная точка смены класса
        /// </summary>
        [Server]
        public void SetClass(string classId)
        {
            if (string.IsNullOrEmpty(classId))
                return;

            if (_classId.Value == classId)
                return;

            _classId.Value = classId;

            // visualId зависит от класса → обновляем сразу
            _visualId.Value = classController.CurrentVisualId;
        }

        /* ================= CLIENT ================= */

        private void OnClassChanged(string oldValue, string newValue, bool asServer)
        {
            if (asServer)
                return;

            if (string.IsNullOrEmpty(newValue))
                return;

            classController?.ApplyClass(newValue);
        }

        private void OnVisualChanged(string oldValue, string newValue, bool asServer)
        {
            if (asServer)
                return;

            if (string.IsNullOrEmpty(newValue))
                return;

            visualController?.ApplyVisual(newValue);
        }
    }
}
