using UnityEngine;
using Features.Quests.Domain;
using Features.Enemy; // твой неймспейс с EnemyHealth

namespace Features.Quests.UnityIntegration.Adapters
{
    public sealed class EnemyQuestEventsAdapter : MonoBehaviour
    {
        [SerializeField] private QuestManagerMB questManager;
        [SerializeField] private string enemyTag = "Enemy";

        private void OnEnable()
        {
            EnemyHealth.GlobalEnemyKilled += HandleEnemyKilled;
        }

        private void OnDisable()
        {
            EnemyHealth.GlobalEnemyKilled -= HandleEnemyKilled;
        }

        private void HandleEnemyKilled(EnemyHealth enemy)
        {
            if (!enemy.CompareTag(enemyTag))
                return;

            questManager?.Service.HandleEvent(new EnemyKilledEvent(enemyTag));
        }
    }
}
