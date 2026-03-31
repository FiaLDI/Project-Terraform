using FishNet;
using FishNet.Managing.Scened;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Features.Player.UnityIntegration;

public class PlayerEcsSyncService : MonoBehaviour
{
    private Entity entity;
    private EntityManager em;

    private Transform playerTransform;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoaded;

        PlayerRegistry.SubscribeLocalPlayerReady(OnPlayerReady);
    }

    private void OnDisable()
    {
        if (InstanceFinder.SceneManager != null)
            InstanceFinder.SceneManager.OnLoadEnd -= OnSceneLoaded;

        PlayerRegistry.UnsubscribeLocalPlayerReady(OnPlayerReady);
    }

    private void OnPlayerReady(PlayerRegistry registry)
    {
        playerTransform = registry.LocalPlayer.transform;

        Debug.Log("[ECS] Local player ready → bind");
        CreateEntity();
    }

    private void OnSceneLoaded(SceneLoadEndEventArgs args)
    {
        Debug.Log("[ECS] Scene loaded → rebind");

        if (playerTransform != null)
            CreateEntity();
    }

    private void CreateEntity()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null) return;

        em = world.EntityManager;

        if (entity != Entity.Null && em.Exists(entity))
            em.DestroyEntity(entity);

        entity = em.CreateEntity(
            typeof(LocalTransform),
            typeof(PlayerTag)
        );

        em.SetComponentData(entity,
            LocalTransform.FromPosition(playerTransform.position));
    }

    private void Update()
    {
        if (playerTransform == null)
            return;

        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null) return;

        em = world.EntityManager;

        if (!em.Exists(entity))
            return;

        em.SetComponentData(entity,
            LocalTransform.FromPosition(playerTransform.position));
    }
}
