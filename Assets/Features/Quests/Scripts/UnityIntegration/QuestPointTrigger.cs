using UnityEngine;
using Features.Quests.Domain;
using Features.Quests.Domain.Behaviours;

namespace Features.Quests.UnityIntegration
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class QuestPointTrigger : MonoBehaviour
    {
        [SerializeField] private string pointId;
        [SerializeField] private QuestManagerMB questManager;

        private void Awake()
        {
            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;
            if (col.radius <= 0f)
                col.radius = 2f;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            questManager?.Service.HandleEvent(new PointReachedEvent(pointId));
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            questManager?.Service.HandleEvent(new PointLeftEvent(pointId));
        }
    }
}
