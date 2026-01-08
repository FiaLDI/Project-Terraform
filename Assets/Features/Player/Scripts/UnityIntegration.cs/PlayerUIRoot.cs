using System;
using Features.Inventory.UnityIntegration;
using UnityEngine;
using Features.Player.UnityIntegration;

namespace Features.Player.UI
{
    
    [DefaultExecutionOrder(-150)]
    public sealed class PlayerUIRoot : MonoBehaviour
    {
        public static PlayerUIRoot I { get; private set; }

        public GameObject BoundPlayer { get; private set; }

        public event Action<GameObject> OnPlayerBound;

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            I = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            PlayerRegistry.SubscribeLocalPlayerReady(OnLocalPlayerReady);
        }

        private void OnDisable()
        {
            PlayerRegistry.UnsubscribeLocalPlayerReady(OnLocalPlayerReady);
        }

        private void OnLocalPlayerReady(PlayerRegistry reg)
        {
            if (BoundPlayer == null && reg.LocalPlayer != null)
                Bind(reg.LocalPlayer);
        }

        /// <summary>
        /// Вызывается из LocalPlayerController.Bind.
        /// </summary>
        public void Bind(GameObject player)
        {
            Debug.Log($"[PlayerUIRoot] Bind called with player={player?.name}");

            BoundPlayer = player;

            var inv = player.GetComponent<InventoryManager>();
            Debug.Log($"[PlayerUIRoot] Found InventoryManager: {inv}");

            if (inv == null)
            {
                Debug.LogError("[PlayerUIRoot] InventoryManager NOT FOUND on " + player.name);
                return;
            }

            Debug.Log("[PlayerUIRoot] Invoking OnPlayerBound");
            OnPlayerBound?.Invoke(player);
        }

        public void Unbind()
        {
            if (BoundPlayer == null)
                return;

            BoundPlayer = null;
            Debug.Log("[PlayerUIRoot] Player unbound");
            OnPlayerBound?.Invoke(null);
        }
    }
}
