using UnityEngine;
using Features.Quests.Domain;
using Features.Quests.Application;
using FishNet.Object;
using Features.Player.UnityIntegration;
using System.Collections.Generic;

namespace Features.Quests.UnityIntegration
{
    [RequireComponent(typeof(SphereCollider))]
    public sealed class QuestPointTrigger : NetworkBehaviour
    {
        [SerializeField] private string pointId;
        private readonly HashSet<int> activeActors = new();

        private void Awake()
        {
            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;

            if (col.radius <= 0f)
                col.radius = 2f;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServerStarted || string.IsNullOrWhiteSpace(pointId))
                return;

            if (!TryResolvePlayer(other, out NetworkPlayer player))
                return;

            if (!activeActors.Add(player.gameObject.GetInstanceID()))
                return;

            QuestEventBus.Publish(
                new PointReachedEvent(
                    player.gameObject,
                    pointId
                )
            );
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsServerStarted || string.IsNullOrWhiteSpace(pointId))
                return;

            if (!TryResolvePlayer(other, out NetworkPlayer player))
                return;

            if (!activeActors.Remove(player.gameObject.GetInstanceID()))
                return;

            QuestEventBus.Publish(
                new PointLeftEvent(
                    player.gameObject,
                    pointId
                )
            );
        }

        private static bool TryResolvePlayer(Collider other, out NetworkPlayer player)
        {
            player = other.GetComponentInParent<NetworkPlayer>();
            return player != null;
        }
    }
}
