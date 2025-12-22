using System;
using Features.Inventory.UnityIntegration;
using UnityEngine;

namespace Features.Player.UI
{
    public sealed class PlayerUIRoot : MonoBehaviour
    {
        public static PlayerUIRoot I { get; private set; }

        private GameObject boundPlayer;

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

        // ======================================================
        // BIND
        // ======================================================

        /// <summary>
        /// Привязка UI к заспавненному PlayerCore
        /// </summary>
        public void Bind(GameObject player)
        {
            if (player == null)
            {
                Debug.LogError("[PlayerUIRoot] Bind called with NULL player");
                return;
            }

            boundPlayer = player;

            var drag = GetComponentInChildren<InventoryDragController>(true);
            if (drag != null)
                drag.BindPlayer(player);

            BroadcastMessage(
                "OnPlayerBound",
                player,
                SendMessageOptions.DontRequireReceiver
            );
            OnPlayerBound?.Invoke(player);
        }

        public GameObject BoundPlayer => boundPlayer;
    }
}
