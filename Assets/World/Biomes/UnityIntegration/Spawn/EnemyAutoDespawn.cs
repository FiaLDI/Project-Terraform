using UnityEngine;
using FishNet;
using FishNet.Object;
using Biomes.UnityIntegration;

public class EnemyAutoDespawn : NetworkBehaviour
{
    [SerializeField] private float despawnDistance = 80f;

    private BiomeEnemySpawner _ownerSpawner;
    private Transform _targetPlayer;

    public void Init(BiomeEnemySpawner spawner, Transform player)
    {
        _ownerSpawner = spawner;
        _targetPlayer = player;
    }

    private void Update()
    {
        if (!IsServer)
            return;

        if (_ownerSpawner == null || _targetPlayer == null)
            return;

        float dist = Vector3.Distance(transform.position, _targetPlayer.position);

        if (dist > despawnDistance)
        {
            InstanceFinder.ServerManager.Despawn(gameObject);
        }
    }
}
