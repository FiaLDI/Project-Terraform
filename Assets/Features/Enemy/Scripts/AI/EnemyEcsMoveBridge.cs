using FishNet.Object;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public sealed class EnemyEcsMoveBridge : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Obstacle Detection")]
    [SerializeField] private LayerMask obstacleMask;

    // === PUBLIC DATA FOR VISUALS ===
    public Vector3 CurrentTarget { get; private set; }

    private Entity entity;
    private EntityManager em;
    private Rigidbody rb;
    private bool initialized;

    public override void OnStartServer()
    {
        base.OnStartServer();

        em = World.DefaultGameObjectInjectionWorld.EntityManager;
        rb = GetComponent<Rigidbody>();

        var binder = GetComponent<EnemyEcsRuntimeBinder>();
        entity = binder.Entity;

        if (rb == null)
        {
            Debug.LogError("[EnemyEcsMoveBridge] Rigidbody NOT FOUND", this);
            return;
        }

        initialized = true;
    }

    private void FixedUpdate()
    {
        if (!initialized || !IsServer)
            return;

        if (!em.Exists(entity))
            return;

        var targetData = em.GetComponentData<EnemyTargetPosition>(entity);

        Vector3 target = new(
            targetData.Value.x,
            targetData.Value.y,
            targetData.Value.z
        );

        // === expose target for visuals ===
        CurrentTarget = target;

        Vector3 pos = rb.position;
        Vector3 dir = target - pos;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
            return;

        Vector3 forward = dir.normalized;

        // === obstacle detection ===
        if (Physics.Raycast(
            rb.position + Vector3.up * 0.5f,
            forward,
            0.6f,
            obstacleMask))
        {
            var blocked = em.GetComponentData<EnemyBlocked>(entity);
            blocked.Value = true;
            em.SetComponentData(entity, blocked);
        }

        Vector3 next =
            pos + forward * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(next);

        // === sync ECS transform ===
        em.SetComponentData(
            entity,
            LocalTransform.FromPosition(next)
        );
    }
}
