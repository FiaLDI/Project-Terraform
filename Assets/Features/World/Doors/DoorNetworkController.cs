using System.Collections.Generic;
using Features.Multiplayer.SceneBinding;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Features.World.Doors
{
    [RequireComponent(typeof(Collider))]
    public sealed class DoorNetworkController : SceneBoundNetworkControllerBase
    {
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private float interactionCooldown = 0.25f;

        private readonly SyncVar<bool> isOpen = new();
        private readonly SyncVar<DoorActivationMode> activationMode = new();

        private readonly HashSet<int> playersInside = new();
        private bool manualOpen;
        private float lastInteractionTime = -999f;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            isOpen.OnChange += OnOpenChanged;
        }

        public override void OnStopNetwork()
        {
            isOpen.OnChange -= OnOpenChanged;
            base.OnStopNetwork();
        }

        protected override void OnServerBoundToView(ISceneBoundView view)
        {
            if (view is DoorView doorView)
                activationMode.Value = doorView.ActivationMode;
        }

        protected override void ServerHandleInteraction(
            SceneBoundInteractionCommand command,
            NetworkConnection sender)
        {
            if (Time.time - lastInteractionTime < interactionCooldown)
            {
                Debug.Log($"[Door] Ignored interaction because of cooldown key={BoundKey}", this);
                return;
            }

            if (activationMode.Value == DoorActivationMode.TriggerOnly)
            {
                Debug.Log($"[Door] Ignored interaction because mode is TriggerOnly key={BoundKey}", this);
                return;
            }

            if (command is not SceneBoundInteractionCommand.Primary
                and not SceneBoundInteractionCommand.Toggle
                and not SceneBoundInteractionCommand.Use)
            {
                Debug.Log($"[Door] Ignored unsupported command={command} key={BoundKey}", this);
                return;
            }

            lastInteractionTime = Time.time;
            manualOpen = !manualOpen;
            Debug.Log($"[Door] Toggled manualOpen={manualOpen} key={BoundKey}", this);
            RefreshOpenState();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServerInitialized || !ServerBindingReady)
                return;

            if (activationMode.Value == DoorActivationMode.InteractOnly)
                return;

            if (!other.CompareTag(playerTag))
                return;

            if (playersInside.Add(GetActorKey(other)))
                RefreshOpenState();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsServerInitialized || !ServerBindingReady)
                return;

            if (activationMode.Value == DoorActivationMode.InteractOnly)
                return;

            if (!other.CompareTag(playerTag))
                return;

            if (playersInside.Remove(GetActorKey(other)))
                RefreshOpenState();
        }

        private static int GetActorKey(Collider other)
        {
            var root = other.attachedRigidbody != null
                ? other.attachedRigidbody.gameObject
                : other.transform.root.gameObject;

            return root.GetInstanceID();
        }

        private void RefreshOpenState()
        {
            bool triggerOpen =
                activationMode.Value != DoorActivationMode.InteractOnly &&
                playersInside.Count > 0;

            bool interactOpen =
                activationMode.Value != DoorActivationMode.TriggerOnly &&
                manualOpen;

            bool next = triggerOpen || interactOpen;

            if (isOpen.Value != next)
            {
                Debug.Log(
                    $"[Door] State changed key={BoundKey} isOpen={next} triggerOpen={triggerOpen} interactOpen={interactOpen}",
                    this
                );
                isOpen.Value = next;
            }
        }

        private void OnOpenChanged(bool prev, bool next, bool asServer)
        {
            ReapplyStateToView(false);
        }

        protected override void OnApplyStateToView(ISceneBoundView view, bool snap)
        {
            if (view is DoorView doorView)
            {
                Debug.Log($"[Door] Apply state to view={doorView.name} key={BoundKey} isOpen={isOpen.Value} snap={snap}", doorView.GameObject);
                doorView.SetOpen(isOpen.Value, snap);
            }
        }
    }
}
