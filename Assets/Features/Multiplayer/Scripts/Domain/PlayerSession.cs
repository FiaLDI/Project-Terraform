using FishNet.Object;
using UnityEngine;
using Features.Quests.Domain;
using System.Collections.Generic;
using System.Linq;

namespace Multiplayer.Domain
{
    public sealed class PlayerSession
    {
        public string PersistentId { get; }
        public int? ClientId { get; private set; }
        public NetworkObject PlayerObject { get; private set; }
        public InventorySaveData InventoryData { get; private set; }
        public QuestPersistenceState QuestData { get; private set; }
        public string CharacterId { get; private set; }
        public string ClassId { get; private set; }
        public int Level { get; private set; }
        public int Experience { get; private set; }
        private readonly List<string> pendingWorldQuestIds = new();
        private readonly List<string> pendingWorldChainIds = new();

        public bool IsOnline => ClientId.HasValue;
        public bool HasInventory =>
            InventoryData != null &&
            InventoryData.bag != null;
        public bool HasQuestData =>
            QuestData != null &&
            QuestData.Initialized;
        public bool HasPendingWorldQuestBootstrap =>
            pendingWorldQuestIds.Count > 0 ||
            pendingWorldChainIds.Count > 0;

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

        public void SetCharacterData(string charId, string classId, int level, int experience)
        {
            CharacterId = charId;
            ClassId = classId;
            SetProgression(level, experience);
        }

        public void SetProgression(int level, int experience)
        {
            Level = PlayerProgressionRules.NormalizeLevel(level);
            Experience = PlayerProgressionRules.NormalizeExperience(experience);
        }

        public void SetInventory(InventorySaveData data)
        {
            if (data == null)
                return;

            if (data.bag == null)
                data.bag = new System.Collections.Generic.List<ItemSaveData>();

            InventoryData = data;
        }

        public void SetQuestData(QuestPersistenceState data)
        {
            QuestData = data;
        }

        public void SetPendingWorldQuestBootstrap(
            IEnumerable<string> questIds,
            IEnumerable<string> chainIds)
        {
            pendingWorldQuestIds.Clear();
            pendingWorldChainIds.Clear();

            if (questIds != null)
            {
                pendingWorldQuestIds.AddRange(
                    questIds
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Distinct());
            }

            if (chainIds != null)
            {
                pendingWorldChainIds.AddRange(
                    chainIds
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Distinct());
            }
        }

        public (List<string> questIds, List<string> chainIds) ConsumePendingWorldQuestBootstrap()
        {
            var questIds = new List<string>(pendingWorldQuestIds);
            var chainIds = new List<string>(pendingWorldChainIds);

            pendingWorldQuestIds.Clear();
            pendingWorldChainIds.Clear();

            return (questIds, chainIds);
        }
    }
}
