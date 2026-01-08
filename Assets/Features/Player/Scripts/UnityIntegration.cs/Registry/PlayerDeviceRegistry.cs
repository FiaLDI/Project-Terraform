using System.Collections.Generic;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    /// <summary>
    /// Хранит устройства (турели, дроны и т.п.), принадлежащие игрокам.
    /// НЕ про UI, НЕ про локальность.
    /// </summary>
    public sealed class PlayerDeviceRegistry : MonoBehaviour
    {
        public static PlayerDeviceRegistry Instance { get; private set; }

        private readonly Dictionary<GameObject, List<GameObject>> _devicesByOwner = new();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        // ======================================================
        // REGISTER
        // ======================================================

        public void RegisterDevice(GameObject owner, GameObject device)
        {
            if (!owner || !device)
                return;

            if (!_devicesByOwner.TryGetValue(owner, out var list))
            {
                list = new List<GameObject>();
                _devicesByOwner[owner] = list;
            }

            list.Add(device);
        }

        public void UnregisterDevice(GameObject owner, GameObject device)
        {
            if (!owner || !device)
                return;

            if (_devicesByOwner.TryGetValue(owner, out var list))
            {
                list.Remove(device);
            }
        }

        // ======================================================
        // QUERY
        // ======================================================

        public IReadOnlyList<GameObject> GetDevices(GameObject owner)
        {
            if (!owner)
                return null;

            return _devicesByOwner.TryGetValue(owner, out var list)
                ? list
                : null;
        }
    }
}
