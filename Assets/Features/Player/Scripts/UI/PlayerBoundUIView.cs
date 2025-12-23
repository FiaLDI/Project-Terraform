using UnityEngine;
using Features.Player.UI;

namespace Features.UI
{
    /// <summary>
    /// Базовый класс для UI, зависящего от локального Player.
    /// Решает ТОЛЬКО bootstrap:
    /// - подписку на PlayerUIRoot
    /// - stateful bind / unbind
    /// </summary>
    public abstract class PlayerBoundUIView : MonoBehaviour
    {
        protected GameObject BoundPlayer { get; private set; }

        protected virtual void OnEnable()
        {
            var root = PlayerUIRoot.I;
            if (root == null)
                return;

            if (root.BoundPlayer != null)
                InternalBind(root.BoundPlayer);

            root.OnPlayerBound += InternalBind;
        }

        protected virtual void OnDisable()
        {
            if (PlayerUIRoot.I != null)
                PlayerUIRoot.I.OnPlayerBound -= InternalBind;

            InternalUnbind();
        }

        // =====================================================
        // INTERNAL
        // =====================================================

        private void InternalBind(GameObject player)
        {
            if (player == BoundPlayer)
                return;

            InternalUnbind();

            BoundPlayer = player;
            if (BoundPlayer != null)
                OnPlayerBound(BoundPlayer);
        }

        private void InternalUnbind()
        {
            if (BoundPlayer != null)
                OnPlayerUnbound(BoundPlayer);

            BoundPlayer = null;
        }

        // =====================================================
        // OVERRIDES
        // =====================================================

        /// <summary>
        /// Вызывается, когда Player готов и забинжен.
        /// </summary>
        protected abstract void OnPlayerBound(GameObject player);

        /// <summary>
        /// Вызывается при unbind / disable / смене Player.
        /// </summary>
        protected virtual void OnPlayerUnbound(GameObject player) { }
    }
}
