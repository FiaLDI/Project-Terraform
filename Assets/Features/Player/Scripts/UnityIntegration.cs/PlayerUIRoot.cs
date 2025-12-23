using System;
using UnityEngine;

namespace Features.Player.UI
{
    /// <summary>
    /// Единственная stateful-точка привязки UI к локальному Player.
    /// НЕ источник данных.
    /// НЕ сервис.
    /// ТОЛЬКО bootstrap / context.
    /// </summary>
    public sealed class PlayerUIRoot : MonoBehaviour
    {
        public static PlayerUIRoot I { get; private set; }

        /// <summary>
        /// Текущий привязанный Player (stateful).
        /// Может быть null (например, до спавна или после despawn).
        /// </summary>
        public GameObject BoundPlayer { get; private set; }

        /// <summary>
        /// Событие привязки Player к UI.
        /// Вызывается:
        /// - при первом Bind
        /// - при перепривязке (respawn)
        /// </summary>
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

        /// <summary>
        /// Единственная точка привязки UI к локальному Player.
        /// Вызывается ТОЛЬКО из LocalPlayerController.
        /// </summary>
        public void Bind(GameObject player)
        {
            if (player == BoundPlayer)
                return;

            BoundPlayer = player;

            Debug.Log(
                player != null
                    ? $"[PlayerUIRoot] Bound to player: {player.name}"
                    : "[PlayerUIRoot] Player unbound"
            );

            OnPlayerBound?.Invoke(player);
        }

        /// <summary>
        /// Явный unbind (на будущее: despawn / spectator / disconnect).
        /// </summary>
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
