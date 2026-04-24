using FishNet.Object;
using UnityEngine;
using System.Collections.Generic;
using FishNet;

public class PlayerSessionNetwork : NetworkBehaviour
{
    [ServerRpc]
    public void RequestReturnToSpawnServerRpc()
    {
        int clientId = Owner != null ? Owner.ClientId : -1;
        var registry = PlayerSpawnRegistry.I;

        if (clientId < 0 || registry == null || !registry.TryGetSpawnPoint(clientId, out var pos, out var rot))
            return;

        var movement = GetComponent<DeterministicMovement>();

        movement.ApplyState(new PlayerState
        {
            Tick = InstanceFinder.TimeManager.Tick,
            Position = pos,
            Velocity = Vector3.zero,
            Yaw = rot.eulerAngles.y,
            Pitch = 0f,
            VerticalVelocity = 0f,
            InternalYaw = rot.eulerAngles.y,
            Grounded = true,
            Crouch = false
        });
    }

    [ServerRpc]
    public void RequestReturnToHubServerRpc()
    {
        ShowHubLoadingObserversRpc();
        SceneTransitionService.ReturnAllPlayersToHub();
    }

    [ServerRpc]
    public void RequestWorldServerRpc(string worldConfigId, List<string> questIds, List<string> chainIds)
    {
        if (SceneTransitionService.IsTransitionPendingFor(SceneTransitionService.NameWorldScene))
        {
            Debug.LogWarning("[PlayerSessionNetwork] Duplicate world request ignored.");
            return;
        }

        int seed = Random.Range(int.MinValue, int.MaxValue);
        var sessions = ServerCompositionRoot.I?.Sessions;

        ServerWorldSession.PendingSeed = seed;
        ServerWorldSession.PendingWorldConfigId = worldConfigId;

        if (sessions != null)
        {
            foreach (var onlineSession in sessions.GetOnlineSessions())
                onlineSession.SetPendingWorldQuestBootstrap(questIds, chainIds);

            ServerWorldSession.PendingQuestIds.Clear();
            ServerWorldSession.PendingChainIds.Clear();
        }
        else
        {
            ServerWorldSession.PendingQuestIds = questIds ?? new List<string>();
            ServerWorldSession.PendingChainIds = chainIds ?? new List<string>();
        }

        ShowWorldLoadingObserversRpc(worldConfigId);

        Debug.Log($"[PlayerSessionNetwork] Generated world seed {seed} for '{worldConfigId}'.");
        SceneTransitionService.LoadWorldScene();
    }

    [ObserversRpc]
    public void ShowHubLoadingObserversRpc()
    {
        LoadingScreenService.ShowHub("Returning players to hub...");
    }

    [ObserversRpc]
    public void ShowWorldLoadingObserversRpc(string worldConfigId)
    {
        LoadingScreenService.ShowWorld(worldConfigId, "Generating procedural world...");
    }
}
