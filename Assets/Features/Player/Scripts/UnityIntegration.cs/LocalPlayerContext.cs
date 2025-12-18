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
        public static GameObject Player
            => PlayerRegistry.Instance?.LocalPlayer;

        public static IInventoryContext Inventory
        {
            get
            {
                if (Player == null)
                {
                    Debug.LogError("[LocalPlayerContext] LocalPlayer is NULL");
                    return null;
                }

                var inv = Player.GetComponent<InventoryManager>();
                if (inv == null)
                {
                    Debug.LogError("[LocalPlayerContext] InventoryManager not found on LocalPlayer");
                    return null;
                }

                return inv;
            }
        }

        public static T Get<T>() where T : Component
        {
            if (Player == null)
                return null;

            return Player.GetComponent<T>();
        }

        internal static void SetInventory(InventoryManager inventoryManager)
        {
            throw new NotImplementedException();
        }
    }
}
