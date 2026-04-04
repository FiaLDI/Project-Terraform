using Features.Buffs.Domain;
using UnityEngine;

namespace Features.Quests.Domain
{
    public struct EnemyKilledEvent : IQuestEvent
    {
        public GameObject Source { get; }
        public string EnemyId;
        public IBuffSource Killer;

        public EnemyKilledEvent(GameObject source, string enemyId, IBuffSource killer)
        {
            Source = source;
            EnemyId = enemyId;
            Killer = killer;
        }
    }
}
