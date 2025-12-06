using UnityEngine;
using System.Collections.Generic;
using Features.Quests.Domain;

namespace Features.Quests.Data
{
    [CreateAssetMenu(menuName = "Quests/Quest Database", fileName = "QuestDatabase")]
    public class QuestDatabaseAsset : ScriptableObject
    {
        [Header("Список всех квестов проекта")]
        [SerializeField] private List<QuestAsset> quests = new();

        // Кэш, чтобы не пересоздавать QuestDefinition каждый раз
        private Dictionary<string, QuestDefinition> _cache;

        /// <summary>
        /// Получить QuestDefinition по QuestId.
        /// </summary>
        public QuestDefinition GetDefinition(QuestId id)
        {
            EnsureCacheBuilt();

            _cache.TryGetValue(id.Value, out var def);
            return def;
        }

        /// <summary>
        /// Получить QuestDefinition по строковому ID.
        /// </summary>
        public QuestDefinition GetDefinition(string id)
        {
            return GetDefinition(new QuestId(id));
        }

        /// <summary>
        /// Построить кэш (лениво).
        /// </summary>
        private void EnsureCacheBuilt()
        {
            if (_cache != null) return;

            _cache = new Dictionary<string, QuestDefinition>();

            foreach (var qa in quests)
            {
                if (qa == null)
                    continue;

                var def = qa.ToDefinition();
                if (def == null)
                    continue;

                _cache[def.Id.Value] = def;
            }
        }
    }
}
