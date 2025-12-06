using UnityEngine;
using System.Collections.Generic;
using Features.Quests.Domain;

namespace Features.Quests.Data
{
    [CreateAssetMenu(menuName = "Quests/Quest Chain")]
    public class QuestChainAsset : ScriptableObject
    {
        [Header("Название цепочки (для UI и редактора)")]
        public string chainName;

        [Header("ID цепочки (генерируется автоматически)")]
        public string chainId;

        [Header("Список квестов по порядку")]
        public List<QuestAsset> quests = new();

        public QuestChainDefinition ToDefinition()
        {
            var defs = new List<QuestDefinition>();

            foreach (var quest in quests)
            {
                if (quest != null)
                    defs.Add(quest.ToDefinition());
            }

            return new QuestChainDefinition(new QuestId(chainId), defs);
        }
    }
}
