using UnityEngine;

namespace Quests
{
    [RequireComponent(typeof(SphereCollider))]
    public class QuestPoint : MonoBehaviour
    {
        public QuestAsset linkedQuest;

        private bool used;

        private void Awake()
        {
            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;
            if (col.radius <= 0f)
                col.radius = 2f;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (used) return;
            if (!other.CompareTag("Player")) return;
            if (linkedQuest == null) return;

            used = true;

            QuestManager.Instance.UpdateQuestProgress(linkedQuest, 1);

            Destroy(gameObject);
        }
    }
}
