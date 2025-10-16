using UnityEngine;

namespace Quests
{
    [System.Serializable]
    public class StandOnPointQuestBehaviour : QuestBehaviour
    {
        public Transform targetPoint;
        public float requiredStayTime = 2f;

        private float stayTimer;
        private bool active;
        private bool completed;

        public override void StartQuest(QuestAsset quest)
        {
            stayTimer = 0f;
            active = true;
            completed = false;
        }

        public override void UpdateProgress(QuestAsset quest, int amount = 1)
        {
            if (!active || completed || targetPoint == null) return;

            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            float dist = Vector3.Distance(player.position, targetPoint.position);

            if (dist < 2f) // радиус
            {
                stayTimer += Time.deltaTime;

                if (stayTimer >= requiredStayTime)
                {
                    completed = true;
                    active = false;
                }

            }
            else
            {
                stayTimer = 0f;
            }
        }

        public override void CompleteQuest(QuestAsset quest)
        {
            // здесь ничего не делаем — завершение фиксирует QuestPoint
        }

        public override void ResetQuest(QuestAsset quest)
        {
            stayTimer = 0f;
            active = false;
            completed = false;
        }

        public override bool IsActive => active;
        public override bool IsCompleted => completed;

        public override int CurrentProgress => 0; // чтобы UI показывал только 0/1
        public override int TargetProgress => 1;

        public override QuestBehaviour Clone() => (StandOnPointQuestBehaviour)MemberwiseClone();
    }
}
