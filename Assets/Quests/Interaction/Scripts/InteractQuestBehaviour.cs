using UnityEngine;
using UnityEngine.InputSystem;

namespace Quests
{
    [System.Serializable]
    public class InteractQuestBehaviour : QuestBehaviour
    {
        public Transform targetPoint;
        public float interactDistance = 2f;

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

            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            float dist = Vector3.Distance(player.position, targetPoint.position);

            if (dist <= interactDistance && Keyboard.current.eKey.wasPressedThisFrame)
            {
                CompleteQuest(quest);
            }
        }

        public override void CompleteQuest(QuestAsset quest)
        {
            if (completed) return;
            completed = true;
            active = false;
        }

        public override void ResetQuest(QuestAsset quest)
        {
            active = false;
            completed = false;
        }

        public override bool IsActive => active;
        public override bool IsCompleted => completed;
        public override int CurrentProgress => completed ? 1 : 0;
        public override int TargetProgress => 1;

        public override QuestBehaviour Clone() => (InteractQuestBehaviour)MemberwiseClone();
    }
}
