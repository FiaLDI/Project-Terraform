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

    private void FixedUpdate()
    {
        if (!IsServer) return;

        TryInitialize();
        if (!initialized || !em.Exists(entity)) return;

        // ================= INACTIVE =================
        if (em.HasComponent<EnemyInactive>(entity))
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        var targetData = em.GetComponentData<EnemyTargetPosition>(entity);

        Vector3 pos = rb.position;
        Vector3 target = targetData.Value;

        // ================= DIRECTION =================
        Vector3 toTarget = target - pos;

        // 👉 работаем в XZ для дистанции
        Vector3 flatDir = toTarget;
        flatDir.y = 0;

        float dist = flatDir.magnitude;

        if (dist < 0.3f)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        // ================= GROUND NORMAL =================
        Vector3 groundNormal = Vector3.up;

        if (Physics.Raycast(
            pos + Vector3.up * 0.5f,
            Vector3.down,
            out RaycastHit hit,
            2f,
            groundMask,
            QueryTriggerInteraction.Ignore))
        {
            groundNormal = hit.normal;
        }

        // 👉 движение вдоль поверхности
        Vector3 moveDir = Vector3.ProjectOnPlane(flatDir, groundNormal).normalized;

        // ================= STEP CHECK =================
        Vector3 forward = moveDir;

        bool blockedLow = Physics.Raycast(
            pos + Vector3.up * 0.1f,
            forward,
            0.5f,
            obstacleMask);

        bool freeHigh = !Physics.Raycast(
            pos + Vector3.up * 0.6f,
            forward,
            0.5f,
            obstacleMask);

        if (blockedLow && freeHigh)
        {
            // 👉 мягкий "шаг"
            rb.AddForce(Vector3.up * 3f, ForceMode.VelocityChange);
        }

        // ================= AVOIDANCE =================
        Collider[] neighbors = Physics.OverlapSphere(pos, 0.8f);

        Vector3 separation = Vector3.zero;

        foreach (var col in neighbors)
        {
            if (col.attachedRigidbody == rb) continue;
            if (!col.CompareTag("Enemy")) continue;

            Vector3 away = pos - col.transform.position;
            away.y = 0;

            float d = away.magnitude;

            if (d > 0.001f)
                separation += away.normalized / d;
        }

        moveDir += separation * 0.3f;
        moveDir.Normalize();

        // ================= SPEED =================
        float speed = moveSpeed;

        if (dist < 1.5f)
            speed *= dist / 1.5f;

        Vector3 vel = moveDir * speed;

        rb.linearVelocity = new Vector3(
            vel.x,
            rb.linearVelocity.y,
            vel.z
        );

        // ================= ROTATION =================
        Vector3 lookDir = moveDir;
        lookDir.y = 0;

        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);

            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation,
                targetRot,
                10f * Time.fixedDeltaTime
            ));
        }

        // ================= SYNC ECS =================
        var transformData = em.GetComponentData<LocalTransform>(entity);
        transformData.Position = rb.position;
        em.SetComponentData(entity, transformData);
    }
}
