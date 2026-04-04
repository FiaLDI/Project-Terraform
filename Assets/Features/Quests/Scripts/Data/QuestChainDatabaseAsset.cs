using UnityEngine;
using System.Collections.Generic;
using Features.Quests.Domain;

namespace Features.Quests.Data
{
    [CreateAssetMenu(menuName = "Quests/Quest Chain Database", fileName = "QuestChainDatabase")]
    public class QuestChainDatabaseAsset : ScriptableObject
    {
        [SerializeField] private List<QuestChainAsset> chains = new();

        private Dictionary<string, QuestChainDefinition> _cache;

        public QuestChainDefinition GetDefinition(string id)
        {
            EnsureCacheBuilt();

            _cache.TryGetValue(id, out var def);
            return def;
        }

        public QuestChainDefinition GetDefinition(QuestId id)
        {
            return GetDefinition(id.Value);
        }

        private void EnsureCacheBuilt()
        {
            if (_cache != null)
                return;

            _cache = new Dictionary<string, QuestChainDefinition>();

            foreach (var chain in chains)
            {
                if (chain == null)
                    continue;

                var def = chain.ToDefinition();

                if (_cache.ContainsKey(def.Id.Value))
                {
                    Debug.LogError($"Duplicate QuestChainId: {def.Id.Value}");
                    continue;
                }

                _cache[def.Id.Value] = def;
            }
        }
    }
}
