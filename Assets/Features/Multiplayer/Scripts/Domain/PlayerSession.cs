using FishNet.Object;
using UnityEngine;

namespace Multiplayer.Domain
{
    public sealed class PlayerSession
    {
        public string PersistentId { get; }
        public int? ClientId { get; private set; }
        public NetworkObject PlayerObject { get; private set; }
        public InventorySaveData InventoryData { get; private set; }
        public string CharacterId { get; private set; }
        public string ClassId { get; private set; }
        public int Level { get; private set; }

        public bool IsOnline => ClientId.HasValue;
        public bool HasInventory =>
            InventoryData != null &&
            InventoryData.bag != null;

        public PlayerSession(string persistentId)
        {
            PersistentId = persistentId;
        }

        public void BindClient(int clientId)
        {
            ClientId = clientId;
        }

        public void UnbindClient()
        {
            Debug.Log($"[fix-net] Session bound to clientId={ClientId}");

            ClientId = null;
        }

        public void SetPlayerObject(NetworkObject obj)
        {
            PlayerObject = obj;
        }

        public void SetCharacterData(string charId, string classId, int level)
        {
            CharacterId = charId;
            ClassId = classId;
            Level = level;
        }

        public void SetInventory(InventorySaveData data)
        {
            if (data == null)
                return;

            if (data.bag == null)
                data.bag = new System.Collections.Generic.List<ItemSaveData>();

            InventoryData = data;
        }
    }
}
