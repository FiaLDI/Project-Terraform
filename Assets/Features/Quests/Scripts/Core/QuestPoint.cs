using UnityEngine;

namespace Quests
{
    [RequireComponent(typeof(SphereCollider))]
    public class QuestPoint : MonoBehaviour
    {
        [Tooltip("Квест, к которому привязана эта цель")]
        public QuestAsset linkedQuest;

        private bool isCompleted;
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
                if (!QuestManager.Instance.activeQuests.Contains(linkedQuest) &&
                    !QuestManager.Instance.completedQuests.Contains(linkedQuest))
                {
                    QuestManager.Instance.StartQuest(linkedQuest);
                    Debug.Log($"Квест '{linkedQuest.questName}' запущен через QuestManager.");
                }

                linkedQuest.OnQuestUpdated += HandleQuestUpdate;

                if (linkedQuest.behaviour != null)
                {
                    localBehaviour = linkedQuest.behaviour.Clone();

                    if (localBehaviour is ApproachPointQuestBehaviour approach)
                    {
                        linkedQuest.RegisterTarget();
                        approach.targetPoint = transform;
                    }
                    else if (localBehaviour is StandOnPointQuestBehaviour stand)
                    {
                        linkedQuest.RegisterTarget();
                        stand.targetPoint = transform;
                    }
                    else if (localBehaviour is InteractQuestBehaviour interact)
                    {
                        linkedQuest.RegisterTarget();
                        interact.targetPoint = transform;
                    }
                }

                Debug.Log($"QuestPoint для квеста '{linkedQuest.questName}' инициализирован. Всего целей: {linkedQuest.targetProgress}");
            }
        }

        private void HandleQuestUpdate(QuestAsset quest)
        {
            if (quest.IsCompleted)
            {
                Debug.Log($"Квест '{quest.questName}' завершен, уничтожаем QuestPoint.");
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (linkedQuest != null)
            {
                linkedQuest.OnQuestUpdated -= HandleQuestUpdate;
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
            else if (localBehaviour is InteractQuestBehaviour interact)
            {
                return interact.IsCompleted;
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