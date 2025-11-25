using UnityEngine;

namespace Quests
{
    [System.Serializable]
    public class ApproachPointQuestBehaviour : QuestBehaviour
    {
        public Transform targetPoint;
        public float requiredDistance = 3f;

        private bool active;
        private bool completed;

        public override void StartQuest(QuestAsset quest)
        {
            active = true;
            completed = false;
        }

        public override void UpdateProgress(QuestAsset quest, int amount = 1)
        {
            if (!active || completed || targetPoint == null) return;

            var player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) return;

            if (Vector3.Distance(player.position, targetPoint.position) <= requiredDistance)
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
        }

        public override QuestBehaviour Clone()
            => (ApproachPointQuestBehaviour)MemberwiseClone();
    }
}
