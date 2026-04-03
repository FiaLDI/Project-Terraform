using UnityEngine;
using Features.Quests.Domain;
using Features.Quests.Application;

namespace Features.Quests.UnityIntegration
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class QuestPointTrigger : MonoBehaviour
    {
        [SerializeField] private string pointId;

        private void Awake()
        {
            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;

            if (col.radius <= 0f)
                col.radius = 2f;
        }

        private void OnTriggerEnter(Collider other)
        {
            var local = LocalPlayerController.I;

            if (local == null || local.BoundPlayer == null)
                return;

            if (other.gameObject != local.BoundPlayer.gameObject)
                return;

            QuestEventBus.Publish(
                new PointReachedEvent(
                    other.gameObject,
                    pointId
                )
            );
        }

        private void OnTriggerExit(Collider other)
        {
            var local = LocalPlayerController.I;

            if (local == null || local.BoundPlayer == null)
                return;

            if (other.gameObject != local.BoundPlayer.gameObject)
                return;

            QuestEventBus.Publish(
                new PointLeftEvent(
                    other.gameObject,
                    pointId
                )
            );
        }
    }
}
