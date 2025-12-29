using FishNet.Managing.Server;
using FishNet.Object;
using UnityEngine;

namespace Features.Network
{
    /// <summary>
    /// 🎯 Централизованный доступ к ServerManager
    /// Используется везде, где нужен сетевой спавн
    /// </summary>
    public sealed class ServerManagerProvider : MonoBehaviour
    {
        public static ServerManager Instance { get; private set; }

        private void Awake()
        {
            // Получаем ServerManager из любого NetworkObject
            var anyNetObj = FindObjectOfType<NetworkObject>();
            if (anyNetObj != null && anyNetObj.ServerManager != null)
            {
                Instance = anyNetObj.ServerManager;
                Debug.Log("[ServerManagerProvider] ServerManager initialized", this);
            }
            else
            {
                Debug.LogWarning("[ServerManagerProvider] No NetworkObject found in scene!", this);
            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}
