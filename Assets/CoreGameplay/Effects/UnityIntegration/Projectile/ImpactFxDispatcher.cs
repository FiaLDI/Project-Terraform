using UnityEngine;
using FishNet.Object;
using Features.Effects.Application;

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

    [Server]
    public void ServerPlaySound(SoundEffectConfig config, Vector3 pos)
    {
        if (config == null)
            return;

        if (string.IsNullOrWhiteSpace(config.id))
        {
            Debug.LogWarning($"[ImpactFxDispatcher] Sound config '{config.name}' has no id. Network playback skipped.");
            return;
        }

        RpcPlaySound(pos, config.id);
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

    [ObserversRpc]
    private void RpcPlaySound(Vector3 pos, string soundId)
    {
        var config = SoundRegistrySO.Get(soundId);
        if (config == null)
        {
            Debug.LogWarning($"[ImpactFxDispatcher] Sound config '{soundId}' not found in SoundRegistry.");
            return;
        }

        SoundEffectPlayer.Instance.Play(
            config,
            pos,
            $"{soundId}:{pos}"
        );
    }
}
