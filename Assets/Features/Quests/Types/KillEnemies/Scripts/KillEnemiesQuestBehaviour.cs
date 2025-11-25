using UnityEngine;

namespace Quests
{
    [System.Serializable]
    public class KillEnemiesQuestBehaviour : QuestBehaviour
    {
        [Tooltip("Тег врага, убийства которого засчитываются в квест")]
        public string enemyTag = "Enemy";

        [Tooltip("Сколько врагов нужно убить")]
        public int requiredKills = 5;

        private int currentKills;
        private bool active;
        private bool completed;
        private QuestAsset myQuest;

        public override void StartQuest(QuestAsset quest)
        {
            myQuest = quest;
            currentKills = 0;
            active = true;
            completed = false;

            if (myQuest != null)
            {
                myQuest.currentProgress = 0;
                myQuest.targetProgress = requiredKills;
                myQuest.NotifyQuestUpdated();
            }
        }

        /// <summary>
        /// Должен вызываться системой боя при смерти врага.
        /// Можно вызывать только для нужных врагов, либо фильтровать по enemyTag снаружи.
        /// </summary>
        public void RegisterKill()
        {
            if (!active || completed)
                return;

            currentKills++;

            if (myQuest != null)
                QuestManager.Instance?.UpdateQuestProgress(myQuest);
        }

        public override void UpdateProgress(QuestAsset quest, int amount = 1)
        {
            if (!active || completed || myQuest == null)
                return;

            // Просто синхронизируем прогресс в QuestAsset
            myQuest.currentProgress = currentKills;
            // QuestManager после этого проверит myQuest.IsCompleted
        }

        public override void CompleteQuest(QuestAsset quest)
        {
            if (completed) return;

            completed = true;
            active = false;

            myQuest?.NotifyQuestUpdated();
        }

        public override void ResetQuest(QuestAsset quest)
        {
            currentKills = 0;
            active = false;
            completed = false;

            if (myQuest != null)
            {
                myQuest.currentProgress = 0;
                myQuest.targetProgress = requiredKills;
                myQuest.NotifyQuestUpdated();
            }

            myQuest = null;
        }

        public override bool IsActive => active;
        public override bool IsCompleted => completed;
        public override int CurrentProgress => currentKills;
        public override int TargetProgress => requiredKills;

        public override QuestBehaviour Clone()
            => (KillEnemiesQuestBehaviour)MemberwiseClone();
    }
}
