using UnityEngine;

namespace Quests
{
    [RequireComponent(typeof(SphereCollider))]
    public class QuestPoint : MonoBehaviour
    {
        [Tooltip("Квест, к которому привязана эта цель")]
        public QuestAsset linkedQuest;

        private bool isCompleted;

        // Важно! Каждая точка имеет свой локальный экземпляр поведения
        private QuestBehaviour localBehaviour;

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
                    localBehaviour = linkedQuest.behaviour.Clone();

                    if (localBehaviour is ApproachPointQuestBehaviour approach)
                        approach.targetPoint = transform;
                    else if (localBehaviour is StandOnPointQuestBehaviour stand)
                        stand.targetPoint = transform;

                    // НЕ переписываем linkedQuest.behaviour!
                }

                Debug.Log($"Новая цель зарегистрирована в квесте '{linkedQuest.questName}'. Всего целей: {linkedQuest.targetProgress}");
            }
        }

        private void Update()
        {
            if (linkedQuest == null || isCompleted) return;

            localBehaviour?.UpdateProgress(linkedQuest);

            if (CheckGoalComplete())
            {
                Complete();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (linkedQuest == null || isCompleted) return;
            if (!other.CompareTag("Player")) return;

            localBehaviour?.UpdateProgress(linkedQuest);

            if (CheckGoalComplete())
                Complete();
        }

        private bool CheckGoalComplete()
        {
            if (localBehaviour is ApproachPointQuestBehaviour approach)
            {
                Transform player = GameObject.FindGameObjectWithTag("Player").transform;
                return Vector3.Distance(player.position, transform.position) <= approach.requiredDistance;
            }
            else if (localBehaviour is StandOnPointQuestBehaviour stand)
            {
                return stand.IsCompleted;
            }

            return localBehaviour == null;
        }

        private void Complete()
        {
            if (isCompleted) return;
            isCompleted = true;

            linkedQuest.TargetCompleted();
            localBehaviour?.CompleteQuest(linkedQuest);

            QuestManager.Instance?.UpdateQuestProgress(linkedQuest);

            Debug.Log($"Цель квеста '{linkedQuest.questName}' выполнена ({linkedQuest.currentProgress}/{linkedQuest.targetProgress})");

            Destroy(gameObject);
        }
    }
}

