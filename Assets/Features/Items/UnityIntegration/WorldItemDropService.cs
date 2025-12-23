using UnityEngine;
using FishNet.Object;
using Features.Items.Domain;

public sealed class WorldItemDropService : NetworkBehaviour
{
    public static WorldItemDropService I { get; private set; }

    public override void OnStartServer()
    {
        I = this;
    }

    public override void OnStopServer()
    {
        if (I == this)
            I = null;
    }


    /// <summary>
    /// SERVER ONLY.
    /// Спавнит world item по ItemInstance.
    /// </summary>
   [Server]
    public void DropServer(ItemInstance inst, Vector3 pos, Vector3 forward)
    {
        if (inst == null || inst.IsEmpty || inst.itemDefinition == null)
        {
            Debug.LogWarning("[DROP] Invalid ItemInstance");
            return;
        }

        var prefab = inst.itemDefinition.worldPrefab;
        if (prefab == null)
        {
            Debug.LogError($"[DROP] Item '{inst.itemDefinition.id}' has no worldPrefab");
            return;
        }

        Vector3 spawnPos = pos + forward.normalized * 1.5f;

        var go = Instantiate(prefab, spawnPos, Quaternion.identity);

        var netObj = go.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("[DROP] worldPrefab has no NetworkObject");
            Destroy(go);
            return;
        }

        var worldItem = go.GetComponent<WorldItemNetwork>();
        if (worldItem == null)
        {
            Debug.LogError("[DROP] worldPrefab has no WorldItemNetwork");
            Destroy(go);
            return;
        }

        Spawn(go);              // ✅ СНАЧАЛА SPAWN
        worldItem.Init(inst);   // ✅ ПОТОМ INIT
    }

}
