using UnityEngine;

namespace Features.Player.UI
{
    public sealed class PlayerUIRoot : MonoBehaviour
    {
        public static PlayerUIRoot I { get; private set; }

        private GameObject boundPlayer;

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

            // Рассылаем всем UI-компонентам
            BroadcastMessage(
                "OnPlayerBound",
                player,
                SendMessageOptions.DontRequireReceiver
            );

            Debug.Log("[PlayerUIRoot] Bound to player: " + player.name);
        }

        public GameObject BoundPlayer => boundPlayer;
    }
}
