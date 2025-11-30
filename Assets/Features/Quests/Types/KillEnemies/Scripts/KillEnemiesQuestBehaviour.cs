using UnityEngine;
using Features.Enemy;

namespace Quests
{
    [System.Serializable]
    public class KillEnemiesQuestBehaviour : QuestBehaviour
    {
        [Tooltip("Тег врагов, убийства которых считаются валидными")]
        public string enemyTag = "Enemy";

        [Tooltip("Нужно убить врагов")]
        public int requiredKills = 5;

        private int currentKills;
        private bool active;
        private bool completed;

        private QuestAsset myQuest;

        public override void StartQuest(QuestAsset quest)
        {
            myQuest = quest;

            currentKills = 0;
            completed = false;
            active = true;

            quest.currentProgress = 0;
            quest.targetProgress = requiredKills;
            quest.NotifyQuestUpdated();

            // глобальная подписка на смерти всех врагов
            EnemyHealth.GlobalEnemyKilled += OnGlobalEnemyKilled;
        }

        private void OnGlobalEnemyKilled(EnemyHealth enemy)
        {
            if (!active || completed)
                return;

            if (!enemy.CompareTag(enemyTag))
                return;

            currentKills++;

            QuestManager.Instance.UpdateQuestProgress(myQuest, 1);

            if (currentKills >= requiredKills)
            {
                completed = true;
                active = false;
            }
        }

        public override void UpdateProgress(QuestAsset quest, int amount = 1)
        {
            quest.currentProgress = currentKills;
        }

        public override void CompleteQuest(QuestAsset quest)
        {
            completed = true;
            active = false;

            // обязательно отписаться
            EnemyHealth.GlobalEnemyKilled -= OnGlobalEnemyKilled;
        }

        public override void ResetQuest(QuestAsset quest)
        {
            currentKills = 0;
            active = false;
            completed = false;

            if (quest != null)
            {
                quest.currentProgress = 0;
                quest.targetProgress = requiredKills;
                quest.NotifyQuestUpdated();
            }

            EnemyHealth.GlobalEnemyKilled -= OnGlobalEnemyKilled;
        }

        public override bool IsActive => active;
        public override bool IsCompleted => completed;
        public override int CurrentProgress => currentKills;
        public override int TargetProgress => requiredKills;

        public override QuestBehaviour Clone()
            => (KillEnemiesQuestBehaviour)MemberwiseClone();
    }
}
