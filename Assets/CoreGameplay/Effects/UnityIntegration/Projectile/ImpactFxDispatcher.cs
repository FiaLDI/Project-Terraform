using UnityEngine;
using FishNet.Object;

public class ImpactFxDispatcher : NetworkBehaviour
{
    public static ImpactFxDispatcher Instance;

    private void Awake()
    {
        Instance = this;
    }

    [Server]
    public void ServerSpawn(Vector3 pos, Vector3 normal, string fxId)
    {
        RpcSpawn(pos, normal, fxId);
    }

    [ObserversRpc]
    private void RpcSpawn(Vector3 pos, Vector3 normal, string fxId)
    {
        var prefab = ImpactFxRegistrySO.Instance.Get(fxId);
        if (prefab == null)
            return;

        var go = ProjectilePool.Instance.Get(
            prefab,
            pos,
            Quaternion.LookRotation(normal)
        );

        var ps = go.GetComponent<ParticleSystem>();
        if (ps != null)
            ps.Play();
    }
}
