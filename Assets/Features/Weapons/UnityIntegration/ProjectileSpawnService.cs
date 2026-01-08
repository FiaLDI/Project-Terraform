using FishNet.Object;
using UnityEngine;
using Features.Weapons.Domain;

public sealed class ProjectileSpawnService : NetworkBehaviour
{
    public static ProjectileSpawnService I { get; private set; }

    public override void OnStartServer()
    {
        base.OnStartServer();
        I = this;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        if (I == this) I = null;
    }

    [Server]
    public void SpawnServer(
        ProjectileConfig cfg,
        Vector3 position,
        Vector3 direction,
        NetworkObject ownerNetObj = null)
    {
        if (cfg == null || cfg.projectilePrefab == null)
            return;

        var rot = Quaternion.LookRotation(direction.normalized);
        var go = Instantiate(cfg.projectilePrefab, position, rot);

        var netObj = go.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("[ProjectileSpawnService] projectilePrefab has no NetworkObject");
            Destroy(go);
            return;
        }

        Spawn(go);

        var proj = go.GetComponent<ProjectileNetwork>();
        if (proj == null)
        {
            Debug.LogError("[ProjectileSpawnService] projectilePrefab has no ProjectileNetwork");
            Despawn(netObj);
            return;
        }

        proj.InitServer(cfg, ownerNetObj);
    }
}
