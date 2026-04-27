using UnityEngine;
using FishNet.Object;
using Features.Effects.Application;

public class ImpactFxDispatcher : NetworkBehaviour
{
    public static ImpactFxDispatcher Instance;

    private static Material chainMaterial;

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

    [Server]
    public void ServerSpawnChain(Vector3 start, Vector3 end, float lifetime = 0.12f)
    {
        RpcSpawnChain(start, end, lifetime);
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

    [ObserversRpc]
    private void RpcSpawnChain(Vector3 start, Vector3 end, float lifetime)
    {
        var go = new GameObject("ChainLightningFx");
        var line = go.AddComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.positionCount = 6;
        for (int i = 0; i < line.positionCount; i++)
        {
            float t = i / (float)(line.positionCount - 1);
            Vector3 pos = Vector3.Lerp(start, end, t);

            if (i > 0 && i < line.positionCount - 1)
                pos += Random.insideUnitSphere * 0.18f;

            line.SetPosition(i, pos);
        }

        line.startWidth = 0.08f;
        line.endWidth = 0.025f;
        line.numCapVertices = 3;
        line.material = GetChainMaterial();
        line.startColor = new Color(0.35f, 0.95f, 1f, 1f);
        line.endColor = new Color(0.95f, 1f, 1f, 0.35f);

        Destroy(go, Mathf.Max(0.02f, lifetime));
    }

    private static Material GetChainMaterial()
    {
        if (chainMaterial != null)
            return chainMaterial;

        var shader =
            Shader.Find("Sprites/Default") ??
            Shader.Find("Universal Render Pipeline/Unlit") ??
            Shader.Find("Hidden/Internal-Colored");

        chainMaterial = new Material(shader);
        return chainMaterial;
    }
}
