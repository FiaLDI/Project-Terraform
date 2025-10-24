namespace Quests
{
    [System.Serializable]
    public class KillEnemiesQuestBehaviour : QuestBehaviour
    {
        public string enemyTag = "Enemy";
        public int requiredKills = 5;

        private int currentKills;
        private bool active;
        private bool completed;

        public override void StartQuest(QuestAsset quest)
        {
            currentKills = 0;
            active = true;
            completed = false;
        }

        public void RegisterKill()
        {
            if (!active || completed) return;
            currentKills++;
        }

        public override void UpdateProgress(QuestAsset quest, int amount = 1)
        {
            if (!active || completed) return;
            if (currentKills >= requiredKills)
                CompleteQuest(quest);
        }

        public override void CompleteQuest(QuestAsset quest)
        {
            if (completed) return;
            completed = true;
            active = false;
        }

        public override void ResetQuest(QuestAsset quest)
        {
            currentKills = 0;
            active = false;
            completed = false;
        }

        public override bool IsActive => active;
        public override bool IsCompleted => completed;
        public override int CurrentProgress => currentKills;
        public override int TargetProgress => requiredKills;

        public override QuestBehaviour Clone() => (KillEnemiesQuestBehaviour)MemberwiseClone();
    }
}
