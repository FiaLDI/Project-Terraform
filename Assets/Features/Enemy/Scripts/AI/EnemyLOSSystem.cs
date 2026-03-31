using FishNet;
using FishNet.Managing.Scened;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Features.Player.UnityIntegration;

public class EnemyLOSSystem : MonoBehaviour
{
    public static EnemyLOSSystem Instance { get; private set; }

    [SerializeField] private LayerMask obstacleMask;

    private EntityManager em;
    private EntityQuery query;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoaded;
        RefreshWorld();
    }

    private void OnDisable()
    {
        if (InstanceFinder.SceneManager != null)
            InstanceFinder.SceneManager.OnLoadEnd -= OnSceneLoaded;
    }

    private void OnSceneLoaded(SceneLoadEndEventArgs args)
    {
        Debug.Log("[EnemyLOSSystem] Scene loaded → refresh ECS");
        RefreshWorld();
    }

    private void RefreshWorld()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null) return;

        em = world.EntityManager;

        query = em.CreateEntityQuery(
            typeof(EnemyTag),
            typeof(LocalTransform),
            typeof(EnemyHasLineOfSight),
            typeof(EnemyAI)
        );
    }

    private void Update()
    {
        var world = World.DefaultGameObjectInjectionWorld;

        if (world == null || !world.IsCreated)
            return;

        if (em != world.EntityManager)
        {
            em = world.EntityManager;

            query = em.CreateEntityQuery(
                typeof(EnemyTag),
                typeof(LocalTransform),
                typeof(EnemyHasLineOfSight),
                typeof(EnemyAI)
            );
        }

        var registry = PlayerRegistry.Instance;

        if (registry == null || registry.LocalPlayer == null)
            return;

        Vector3 playerPos = registry.LocalPlayer.transform.position;

        var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);

        foreach (var entity in entities)
        {
            if (!em.Exists(entity)) continue;

            var transform = em.GetComponentData<LocalTransform>(entity);
            var ai = em.GetComponentData<EnemyAI>(entity);

            Vector3 enemyPos = transform.Position;

            Vector3 dir = playerPos - enemyPos;
            float dist = dir.magnitude;

            if (dist < 0.001f)
                continue;

            dir /= dist;

            bool blocked = false;

            if (ai.RequireLOS)
            {
                blocked = Physics.Raycast(
                    enemyPos + Vector3.up * 1f,
                    dir,
                    dist,
                    obstacleMask
                );
            }

            var los = em.GetComponentData<EnemyHasLineOfSight>(entity);
            los.Value = !blocked;

            em.SetComponentData(entity, los);
        }

        entities.Dispose();
    }
}
