using UnityEngine;
using FishNet.Object;
using Features.Items.Domain;

public sealed class WorldItemDropService : NetworkBehaviour
{
    public static WorldItemDropService I { get; private set; }

    public override void OnStartServer()
    {
        I = this;
        Debug.Log("[WorldItemDropService] Initialized as server", this);
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
        if (inst == null)
        {
            Debug.LogError("[DROP] DropServer: ItemInstance is NULL", this);
            return;
        }

        if (inst.IsEmpty)
        {
            Debug.LogWarning("[DROP] DropServer: ItemInstance is EMPTY", this);
            return;
        }

        if (inst.itemDefinition == null)
        {
            Debug.LogError("[DROP] DropServer: itemDefinition is NULL", this);
            return;
        }

        if (string.IsNullOrEmpty(inst.itemDefinition.id))
        {
            Debug.LogError("[DROP] DropServer: itemDefinition.id is NULL or EMPTY!", this);
            return;
        }

        if (inst.itemDefinition.id == "0")
        {
            Debug.LogError("[DROP] DropServer: itemDefinition.id is '0' - INVALID!", this);
            return;
        }

        var prefab = inst.itemDefinition.worldPrefab;
        if (prefab == null)
        {
            Debug.LogError($"[DROP] DropServer: Item '{inst.itemDefinition.id}' has no worldPrefab", this);
            return;
        }

        if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsNaN(pos.z))
        {
            Debug.LogError($"[DROP] DropServer: Position contains NaN: {pos}", this);
            return;
        }

        Vector3 spawnPos = pos + forward.normalized * 1.5f;

        Debug.Log($"[DROP] DropServer START: ItemId='{inst.itemDefinition.id}' ({inst.itemDefinition.name}), Qty={inst.quantity}, Level={inst.level}", this);

        var go = Instantiate(prefab, spawnPos, Quaternion.identity, null);

        if (go == null)
        {
            Debug.LogError("[DROP] DropServer: Instantiate failed!", this);
            return;
        }

        var netObj = go.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("[DROP] DropServer: worldPrefab has no NetworkObject", this);
            Destroy(go);
            return;
        }

        var worldItem = go.GetComponent<WorldItemNetwork>();
        if (worldItem == null)
        {
            Debug.LogError("[DROP] DropServer: worldPrefab has no WorldItemNetwork", this);
            Destroy(go);
            return;
        }

        Debug.Log($"[DROP] Before Spawn: go.name={go.name}, netObj.ObjectId={netObj.ObjectId}, parent={go.transform.parent}", this);

        Spawn(go);
        
        Debug.Log($"[DROP] After Spawn: ObjectId={netObj.ObjectId}, Spawned dict size={NetworkManager.ServerManager.Objects.Spawned.Count}", this);

        if (NetworkManager.ServerManager.Objects.Spawned.TryGetValue(netObj.ObjectId, out var spawnedObj))
        {
            Debug.Log($"[DROP] ✅ Object registered in Spawned: ObjectId={netObj.ObjectId}", this);
        }
        else
        {
            Debug.LogError($"[DROP] ❌ Object NOT in Spawned! ObjectId={netObj.ObjectId}", this);
            return;
        }

        Debug.Log($"[DROP] Calling Init() with: ItemId='{inst.itemDefinition.id}', Qty={inst.quantity}, Level={inst.level}", this);
        
        worldItem.Init(inst);
        
        Debug.Log($"[DROP] ✅ Initialized world item with {inst.itemDefinition.id} x{inst.quantity} L{inst.level}, ObjectId={netObj.ObjectId}", this);
    }
}