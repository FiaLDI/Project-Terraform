using UnityEngine;
using Features.Inventory;
using Features.Inventory.UnityIntegration;
using Features.Player.UnityIntegration;
using System;

namespace Features.Player
{
    /// <summary>
    /// Единственная точка доступа к локальному игроку.
    /// UI / Stations / Input слой.
    /// </summary>
    public static class LocalPlayerContext
    {
        public static GameObject Player => PlayerRegistry.Instance?.LocalPlayer;

        public static InventoryManager Inventory
            => PlayerRegistry.Instance?.LocalInventory;

        public static bool IsReady => Player != null;

        /// <summary>
        /// Вызывается, когда локальный игрок полностью зарегистрирован
        /// </summary>
        public static event Action OnReady;

        static LocalPlayerContext()
        {
            PlayerRegistry.OnLocalPlayerReady += _ =>
            {
                OnReady?.Invoke();
            };
        }

        public static T Get<T>() where T : Component
        {
            if (!IsReady)
                return null;

            return Player.GetComponent<T>();
        }
    }
}
