using UnityEngine;

namespace Quests
{
    [System.Serializable]
    public class ProceduralPointsQuestBehaviour : QuestBehaviour
    {
        private bool active;
        private bool completed;

        public override void StartQuest(QuestAsset quest)
        {
            active = true;
            completed = false;
            quest.currentProgress = 0;
        }

        public override void UpdateProgress(QuestAsset quest, int amount = 1)
        {
            if (!active || completed) return;

            quest.currentProgress += amount;

            if (quest.currentProgress >= quest.targetProgress)
            {
                CompleteQuest(quest);
            }
        }

        public override void CompleteQuest(QuestAsset quest)
        {
            completed = true;
            active = false;
        }

        public override void ResetQuest(QuestAsset quest)
        {
            active = false;
            completed = false;
            quest.currentProgress = 0;
        }

        public override bool IsActive => active;
        public override bool IsCompleted => completed;
        public override int CurrentProgress => 0;
        public override int TargetProgress => 0;

        public override QuestBehaviour Clone()
            => (ProceduralPointsQuestBehaviour)MemberwiseClone();
    }
}
