using Features.Quests.Application;
using Features.Quests.Domain;
using Features.Player.UnityIntegration;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Multiplayer.Domain;
using UnityEngine;

namespace Features.Quests.UnityIntegration
{
    [RequireComponent(typeof(Collider))]
    public sealed class QuestInteractionObjective : NetworkBehaviour, IInteractable
    {
        [SerializeField] private string pointId;
        [SerializeField] private string interactionPrompt = "Взаимодействовать";
        [SerializeField] private bool consumeOnce = true;
        [SerializeField] private GameObject completionVisualRoot;

        private readonly SyncVar<bool> completed = new();

        public string InteractionPrompt => completed.Value ? "Цель выполнена" : interactionPrompt;

        private void Awake()
        {
            ApplyCompletedState(completed.Value);
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            completed.OnChange += OnCompletedChanged;
            ApplyCompletedState(completed.Value);
        }

        public override void OnStopNetwork()
        {
            completed.OnChange -= OnCompletedChanged;
            base.OnStopNetwork();
        }

        public bool Interact()
        {
            if (completed.Value || string.IsNullOrWhiteSpace(pointId))
                return false;

            RequestInteractServerRpc();
            return true;
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestInteractServerRpc(NetworkConnection sender = null)
        {
            if (completed.Value || string.IsNullOrWhiteSpace(pointId))
                return;

            NetworkPlayer player = ResolvePlayer(sender);
            if (player == null)
                return;

            if (consumeOnce)
                completed.Value = true;

            QuestEventBus.Publish(
                new InteractionEvent(
                    player.gameObject,
                    pointId
                )
            );
        }

        private void OnCompletedChanged(bool previous, bool next, bool asServer)
        {
            ApplyCompletedState(next);
        }

        private static NetworkPlayer ResolvePlayer(NetworkConnection sender)
        {
            if (sender == null)
                return null;

            PlayerSession session = ServerCompositionRoot.I?.Sessions?.GetSessionByClient(sender.ClientId);
            return session?.PlayerObject != null
                ? session.PlayerObject.GetComponent<NetworkPlayer>()
                : null;
        }

        private void ApplyCompletedState(bool isCompleted)
        {
            if (completionVisualRoot != null)
                completionVisualRoot.SetActive(!isCompleted);
        }
    }
}
