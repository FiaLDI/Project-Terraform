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

        /* ================= LIFETIME ================= */

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

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

            var classCtrl = GetComponent<PlayerClassController>();

            _classId.Value = classCtrl != null
                ? classCtrl.CurrentClassId
                : null;

            _visualId.Value = classCtrl != null
                ? classCtrl.CurrentVisualId
                : null;

            Debug.Log($"[PlayerStateNetwork] Init state class={_classId.Value}, visual={_visualId.Value}");
        }

        /* ================= CLIENT ================= */

        private void OnClassChanged(string oldValue, string newValue, bool asServer)
        {
            if (string.IsNullOrEmpty(newValue))
                return;

            GetComponent<PlayerClassController>()?.SetClass(newValue);
        }

        private void OnVisualChanged(string oldValue, string newValue, bool asServer)
        {
            if (string.IsNullOrEmpty(newValue))
                return;

            GetComponent<PlayerVisualController>()?.ApplyVisual(newValue);
        }
    }
}
