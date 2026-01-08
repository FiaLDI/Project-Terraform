using UnityEngine;
using FishNet.Object;
using FishNet.Managing;
using FishNet;
using System.Collections;

public sealed class NetworkAutoDespawn : NetworkBehaviour
{
    public void StartDespawn(float delay)
    {
        if (!IsServer)
            return;

        StartCoroutine(DespawnRoutine(delay));
    }

    private IEnumerator DespawnRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (IsSpawned)
            InstanceFinder.ServerManager.Despawn(gameObject);
    }
}
