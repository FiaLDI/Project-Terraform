using UnityEngine;

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

            SubscribeToExistingEnemies();
        }

        private void SubscribeToExistingEnemies()
        {
            var enemies = GameObject.FindGameObjectsWithTag(enemyTag);

            foreach (var e in enemies)
            {
                var hp = e.GetComponent<EnemyHealth>();
                if (hp != null)
                    hp.OnEnemyKilled += OnEnemyKilled;
            }
        }

        private void OnEnemyKilled(EnemyHealth enemy)
        {
            if (!active || completed) return;

            currentKills++;

            // Сохраняем прогресс
            myQuest.currentProgress = currentKills;
            myQuest.NotifyQuestUpdated();

            if (currentKills >= requiredKills)
            {
                completed = true;
                active = false;

                QuestManager.Instance.UpdateQuestProgress(myQuest);
            }
        }

        public override void UpdateProgress(QuestAsset quest, int amount = 1)
        {
            // Синхронизация на всякий случай
            quest.currentProgress = currentKills;
        }

        public override void CompleteQuest(QuestAsset quest)
        {
            completed = true;
            active = false;
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
        }

        public override bool IsActive => active;
        public override bool IsCompleted => completed;
        public override int CurrentProgress => currentKills;
        public override int TargetProgress => requiredKills;

        public override QuestBehaviour Clone()
            => (KillEnemiesQuestBehaviour)MemberwiseClone();
    }
}
