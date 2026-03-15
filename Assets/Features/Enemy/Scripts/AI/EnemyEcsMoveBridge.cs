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
    [SerializeField] private LayerMask groundMask;

    public Vector3 CurrentTarget { get; private set; }
    public float CurrentSpeed { get; private set; }

    private Entity entity;
    private EntityManager em;
    private Rigidbody rb;
    private bool initialized;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.constraints =
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        Debug.Log("[Bridge] OnStartServer CALLED", this);

        em = World.DefaultGameObjectInjectionWorld.EntityManager;
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("[Bridge] Rigidbody NOT FOUND", this);
        }
    }

    private void TryInitialize()
    {
        if (initialized) return;

        var binder = GetComponent<EnemyEcsRuntimeBinder>();
        if (binder == null) return;

        if (binder.Entity == Entity.Null) return;
        if (!em.Exists(binder.Entity)) return;

        entity = binder.Entity;

        Debug.Log($"[Bridge] INIT SUCCESS entity={entity.Index}", this);

        initialized = true;
    }

    private void EnsureRigidbody()
    {
        if (rb == null) return;

        if (rb.isKinematic)
        {
            Debug.LogWarning("[Bridge] Rigidbody was kinematic → FIXED", this);
            rb.isKinematic = false;
        }
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        //EnsureRigidbody();

        TryInitialize();

        if (!initialized) return;
        if (!em.Exists(entity)) return;

        var targetData = em.GetComponentData<EnemyTargetPosition>(entity);

        Vector3 target = new Vector3(
            targetData.Value.x,
            targetData.Value.y,
            targetData.Value.z
        );

        CurrentTarget = target;

        Vector3 pos = rb.position;
        Vector3 dir = target - pos;
        dir.y = 0f;


        Debug.Log($"[MOVE] pos={pos} target={target} dist={(target - pos).magnitude}", this);

        float sqr = dir.sqrMagnitude;

        if (sqr < 0.01f)
        {
            CurrentSpeed = 0f;
            return;
        }

        Vector3 forward = dir.normalized;

        // obstacle detection
        if (Physics.Raycast(
            rb.position + Vector3.up * 0.5f,
            forward,
            0.6f,
            obstacleMask))
        {
            if (em.HasComponent<EnemyBlocked>(entity))
            {
                var blocked = em.GetComponentData<EnemyBlocked>(entity);
                blocked.Value = true;
                em.SetComponentData(entity, blocked);
            }
        }

        Vector3 next =
            pos + forward * moveSpeed * Time.fixedDeltaTime;

        // 🔥 фиксируем Y ТОЛЬКО по земле
        if (Physics.Raycast(
            next + Vector3.up * 1.5f,
            Vector3.down,
            out RaycastHit hit,
            3f,
            groundMask,
            QueryTriggerInteraction.Ignore))
        {
            // маленький offset чтобы не залипать
            next.y = hit.point.y + 0.05f;
        }
        else
        {
            // если не нашли землю — не меняем высоту
            next.y = pos.y;
        }

        rb.MovePosition(next);

        Vector3 realPos = rb.position;

        em.SetComponentData(
            entity,
            LocalTransform.FromPosition(realPos)
        );
    }
}
