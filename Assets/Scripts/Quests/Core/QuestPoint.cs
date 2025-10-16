using UnityEngine;

namespace Quests
{
    [RequireComponent(typeof(SphereCollider))]
    public class QuestPoint : MonoBehaviour
    {
        [Tooltip("Квест, к которому привязана эта цель")]
        public QuestAsset linkedQuest;

        private bool isCompleted;

        private void Awake()
        {
            SphereCollider col = GetComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 2f;
        }

        private void Start()
        {
            if (linkedQuest != null)
            {
                linkedQuest.RegisterTarget();

                // создаём копию поведения под этот QuestPoint
                if (linkedQuest.behaviour != null)
                {
                    var clone = linkedQuest.behaviour.Clone();

                    if (clone is ApproachPointQuestBehaviour approach)
                        approach.targetPoint = transform;
                    else if (clone is StandOnPointQuestBehaviour stand)
                        stand.targetPoint = transform;

                    linkedQuest.behaviour = clone;
                }

                Debug.Log($"Новая цель зарегистрирована в квесте '{linkedQuest.questName}'. Всего целей: {linkedQuest.targetProgress}");
            }
        }


        private void Update()
        {
            if (linkedQuest == null || isCompleted) return;

            linkedQuest.behaviour?.UpdateProgress(linkedQuest);

            if (CheckGoalComplete())
            {
                Complete();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (linkedQuest == null || isCompleted) return;
            if (!other.CompareTag("Player")) return;

            linkedQuest.behaviour?.UpdateProgress(linkedQuest);

            if (CheckGoalComplete())
                Complete();
        }

        private bool CheckGoalComplete()
        {
            if (linkedQuest.behaviour is ApproachPointQuestBehaviour approach)
            {
                Transform player = GameObject.FindGameObjectWithTag("Player").transform;
                return Vector3.Distance(player.position, transform.position) <= approach.requiredDistance;
            }
            else if (linkedQuest.behaviour is StandOnPointQuestBehaviour stand)
            {
                return stand.IsCompleted;
            }

            return linkedQuest.behaviour == null;
        }

        private void Complete()
        {
            if (isCompleted) return;
            isCompleted = true;

            linkedQuest.TargetCompleted();
            linkedQuest.behaviour?.CompleteQuest(linkedQuest);

            QuestManager.Instance?.UpdateQuestProgress(linkedQuest);

            Debug.Log($"Цель квеста '{linkedQuest.questName}' выполнена ({linkedQuest.currentProgress}/{linkedQuest.targetProgress})");

            Destroy(gameObject);
        }
    }
}
