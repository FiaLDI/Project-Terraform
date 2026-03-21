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

    private void Start()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;

        query = em.CreateEntityQuery(
            typeof(EnemyTag),
            typeof(LocalTransform),
            typeof(EnemyHasLineOfSight),
            typeof(EnemyAI)
        );
    }

    private void Update()
    {
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
