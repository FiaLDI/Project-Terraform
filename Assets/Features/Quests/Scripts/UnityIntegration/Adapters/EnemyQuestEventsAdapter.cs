using UnityEngine;
using Features.Quests.Domain;
using Features.Enemy;
using Features.Enemy.UnityIntegration;

namespace Features.Quests.UnityIntegration.Adapters
{
    public sealed class EnemyQuestEventsAdapter : MonoBehaviour
    {
        [SerializeField] private QuestManagerMB questManager;

        private void OnEnable()
        {
            //EnemyHealth.GlobalEnemyKilled += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            //EnemyHealth.GlobalEnemyKilled -= HandleEnemyKilled;
        }

        private void HandleEnemyKilled(EnemyHealth enemy)
        {
            // Отправляем именно enemy.EnemyId
            //questManager?.Service.HandleEvent(
            //    new EnemyKilledEvent(enemy.EnemyId)
            //);
        }
    }
}
