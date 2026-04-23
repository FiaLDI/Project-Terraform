using FishNet.Object;
using UnityEngine;
using System.Collections.Generic;
using FishNet;

public class PlayerSessionNetwork : NetworkBehaviour
{
    [ServerRpc]
    public void RequestReturnToSpawnServerRpc()
    {
        if (!PlayerSpawnRegistry.I.TryGetSpawnPoint(out var pos, out var rot))
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
        GetComponent<PlayerQuestComponent>()?.ClearAll();

        var session = ServerCompositionRoot.I?.Sessions?.GetSessionByClient(Owner.ClientId);
        session?.SetPendingWorldQuestBootstrap(null, null);

        SceneTransitionService.LoadHubScene();
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
        var session = ServerCompositionRoot.I?.Sessions?.GetSessionByClient(Owner.ClientId);

        ServerWorldSession.PendingSeed = seed;
        ServerWorldSession.PendingWorldConfigId = worldConfigId;

        if (session != null)
        {
            session.SetPendingWorldQuestBootstrap(questIds, chainIds);
            ServerWorldSession.PendingQuestIds.Clear();
            ServerWorldSession.PendingChainIds.Clear();
        }
        else
        {
            ServerWorldSession.PendingQuestIds = questIds ?? new List<string>();
            ServerWorldSession.PendingChainIds = chainIds ?? new List<string>();
        }

        Debug.Log($"[PlayerSessionNetwork] Generated world seed {seed} for '{worldConfigId}'.");
        SceneTransitionService.LoadWorldScene();
    }
}
