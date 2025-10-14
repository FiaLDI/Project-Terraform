using UnityEngine;

namespace Quests
{
    [RequireComponent(typeof(SphereCollider))]
    public class QuestPoint : MonoBehaviour
    {
        [Header(" акой квест св€зан с этой точкой")]
        public Quest linkedQuest;

        [Header("“ип активации")]
        public bool autoCompleteOnEnter = true;  
        public float requiredStayTime = 0f;      

        private float stayTimer;
        private bool playerInside;

        private void Reset()
        {
            SphereCollider col = GetComponent<SphereCollider>();
            if (col != null)
                col.isTrigger = true;
        }

        private void Start()
        {
            SphereCollider col = GetComponent<SphereCollider>();
            if (col != null && !col.isTrigger)
            {
                Debug.LogWarning($"QuestPoint {gameObject.name}: Collider должен быть Is Trigger!");
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            playerInside = true;
            stayTimer = 0f;

            Debug.Log($"[QuestPoint] »грок вошЄл в точку {gameObject.name}");

            if (autoCompleteOnEnter && linkedQuest != null && linkedQuest.isActive)
            {
                CompleteCurrentQuest();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag("Player") || requiredStayTime <= 0f) return;
            if (linkedQuest == null || !linkedQuest.isActive || linkedQuest.isCompleted) return;

            stayTimer += Time.deltaTime;
            Debug.Log($"[QuestPoint] »грок стоит в точке: {stayTimer:F1}/{requiredStayTime} сек");

            if (stayTimer >= requiredStayTime)
            {
                Debug.Log($"[QuestPoint] »грок просто€л {requiredStayTime} сек Ч завершаем квест");
                CompleteCurrentQuest();
                playerInside = false;
                stayTimer = 0f;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInside = false;
                stayTimer = 0f;
                Debug.Log($"[QuestPoint] »грок вышел из точки {gameObject.name}");
            }
        }

        private void CompleteCurrentQuest()
        {
            if (linkedQuest != null && linkedQuest.isActive)
            {
                QuestManager.Instance.UpdateQuestProgress(linkedQuest);
                Debug.Log($"[QuestPoint]  вест '{linkedQuest.questName}' завершен через точку {gameObject.name}");

            }
            else
            {
                if (linkedQuest == null)
                {
                    Debug.LogWarning($"[QuestPoint] Linked Quest не назначен на точке {gameObject.name}");
                }
                else if (!linkedQuest.isActive)
                {
                    Debug.LogWarning($"[QuestPoint]  вест '{linkedQuest.questName}' не активен на точке {gameObject.name}");
                }
            }
        }

        private void OnDrawGizmos()
        {
            SphereCollider col = GetComponent<SphereCollider>();
            if (col != null)
            {
                Gizmos.color = linkedQuest != null ? Color.green : Color.red;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(Vector3.zero, col.radius);
            }
        }

        private void OnDrawGizmosSelected()
        {
            SphereCollider col = GetComponent<SphereCollider>();
            if (col != null)
            {
                Gizmos.color = linkedQuest != null ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawSphere(Vector3.zero, col.radius);
            }
        }
    }
}