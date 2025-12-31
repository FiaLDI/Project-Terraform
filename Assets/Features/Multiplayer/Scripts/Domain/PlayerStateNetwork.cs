using Features.Class.Net;
using Features.Classes.Data;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    public sealed class PlayerStateNetwork : NetworkBehaviour
    {
        private readonly SyncVar<string> _classId = new();
        private readonly SyncVar<string> _visualId = new();

        private PlayerVisualController visualController;
        private PlayerStateNetAdapter netAdapter;

        private PlayerClassLibrarySO classLibrary;
        private string _preInitClass;

        private void Awake()
        {
            visualController = GetComponent<PlayerVisualController>();
            netAdapter = GetComponent<PlayerStateNetAdapter>();

            classLibrary = UnityEngine.Resources.Load<PlayerClassLibrarySO>(
                "Databases/PlayerClassLibrary");

            if (classLibrary == null)
                Debug.LogError(
                    "[PSN] PlayerClassLibrary not found in Resources/Databases");
        }

        // вызывается ИЗ NetworkPlayerService ДО Spawn
        public void PreInitClass(string classId)
        {
            _preInitClass = classId;
        }

        public override void OnSpawnServer(NetworkConnection conn)
        {
            base.OnSpawnServer(conn);

            string classId = string.IsNullOrEmpty(_preInitClass)
                ? "0" // дефолтный класс
                : _preInitClass;

            var cls = classLibrary.FindById(classId);
            if (cls == null)
            {
                Debug.LogError($"[PSN] Class '{classId}' not found", this);
                return;
            }

            _classId.Value = classId;
            _visualId.Value = cls.visualPreset.id;

            netAdapter.ApplyClass(classId);

            Debug.Log($"[PSN] Spawn class={classId} visual={_visualId.Value}", this);
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            _visualId.OnChange += OnVisualChanged;
        }

        public override void OnStopNetwork()
        {
            _visualId.OnChange -= OnVisualChanged;
        }

        private void OnVisualChanged(string oldVal, string newVal, bool _)
        {
            if (!string.IsNullOrEmpty(newVal))
                visualController.ApplyVisual(newVal);
        }

        [Server]
        public void SetClass(string classId)
        {
            var cls = classLibrary.FindById(classId);
            if (cls == null)
                return;

            _classId.Value = classId;
            _visualId.Value = cls.visualPreset.id;

            netAdapter.ApplyClass(classId);
        }
    }
}
