using Features.Buffs.Domain;
using UnityEngine;

namespace Features.Quests.Domain
{
    public struct EnemyKilledEvent : IQuestEvent
    {
        public string EnemyId;
        public IBuffSource Killer;

        public EnemyKilledEvent(string enemyId, IBuffSource killer)
        {
            EnemyId = enemyId;
            Killer = killer;
        }
    }
}