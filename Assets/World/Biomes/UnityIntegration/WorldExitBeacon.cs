using System.Collections.Generic;
using Features.Player.UnityIntegration;
using Features.Quests.Domain;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace Biomes.UnityIntegration
{
    public sealed class WorldExitBeacon : MonoBehaviour
    {
        private readonly HashSet<int> playersInside = new();
        private bool transitionRequested;

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServerReady() || !TryGetClientId(other, out int clientId))
                return;

            playersInside.Add(clientId);
            TryReturnToHub();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!TryGetClientId(other, out int clientId))
                return;

            playersInside.Remove(clientId);
        }

        private void TryReturnToHub()
        {
            if (transitionRequested)
                return;

            var registry = PlayerRegistry.Instance;
            if (registry == null || registry.Players.Count == 0)
                return;

            for (int i = 0; i < registry.Players.Count; i++)
            {
                var player = registry.Players[i];
                if (player == null)
                    continue;

                var nob = player.GetComponent<NetworkObject>();
                if (nob == null || nob.Owner == null)
                    continue;

                if (!playersInside.Contains(nob.Owner.ClientId))
                    return;
            }

            if (!AreAllPlayersReadyForExtraction(registry))
                return;

            AwardRunCompletionExperience();
            transitionRequested = true;
            SceneTransitionService.ReturnAllPlayersToHub();
        }

        private static bool AreAllPlayersReadyForExtraction(PlayerRegistry registry)
        {
            if (registry == null)
                return false;

            for (int i = 0; i < registry.Players.Count; i++)
            {
                var player = registry.Players[i];
                if (player == null)
                    continue;

                var quests = player.GetComponent<PlayerQuestComponent>();
                if (quests == null)
                {
                    Debug.LogWarning("[WorldExitBeacon] PlayerQuestComponent missing; extraction blocked.");
                    return false;
                }

                if (!quests.AreAllStartedQuestsCompleted())
                {
                    Debug.Log("[WorldExitBeacon] Extraction blocked until all started quests are completed.");
                    return false;
                }
            }

            return true;
        }

        private void AwardRunCompletionExperience()
        {
            var runConfig = WorldRunContext.Current;
            if (runConfig == null)
                return;

            var sessions = ServerCompositionRoot.I?.Sessions;
            var registry = PlayerRegistry.Instance;
            if (sessions == null || registry == null)
                return;

            for (int i = 0; i < registry.Players.Count; i++)
            {
                var player = registry.Players[i];
                if (player == null)
                    continue;

                var nob = player.GetComponent<NetworkObject>();
                if (nob == null || nob.Owner == null)
                    continue;

                var session = sessions.GetSessionByClient(nob.Owner.ClientId);
                if (session == null)
                    continue;

                int gainedExperience = runConfig.GetCompletionExperience(session.Level);
                int nextLevel = session.Level;
                int nextExperience = session.Experience;

                PlayerProgressionRules.ApplyExperience(
                    ref nextLevel,
                    ref nextExperience,
                    gainedExperience);

                session.SetProgression(nextLevel, nextExperience);

                var sessionNet = player.GetComponent<PlayerSessionNetwork>();
                sessionNet?.ServerApplyRunCompletionExperience(
                    nextLevel,
                    nextExperience,
                    gainedExperience);
            }
        }

        private static bool TryGetClientId(Collider other, out int clientId)
        {
            clientId = -1;

            var player = other.GetComponentInParent<NetworkPlayer>();
            if (player == null)
                return false;

            var nob = player.GetComponent<NetworkObject>();
            if (nob == null || nob.Owner == null)
                return false;

            clientId = nob.Owner.ClientId;
            return clientId >= 0;
        }

        private static bool IsServerReady()
        {
            var nm = InstanceFinder.NetworkManager;
            return nm != null && nm.IsServerStarted;
        }
    }
}
