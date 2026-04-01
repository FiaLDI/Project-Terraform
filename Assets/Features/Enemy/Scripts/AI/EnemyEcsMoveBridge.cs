using FishNet.Object;
using Unity.Entities;
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

    // 👉 анти-спам логов
    private float logTimer;

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

        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null)
            em = world.EntityManager;

        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("[ECS-TEST] Rigidbody NOT FOUND", this);
        }
        else
        {
            Debug.Log("[ECS-TEST] Rigidbody OK", this);
        }
    }

    private void TryInitialize()
    {
        if (initialized) return;

        if (em == null)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;

            em = world.EntityManager;
        }

        var binder = GetComponent<EnemyEcsRuntimeBinder>();
        if (binder == null)
        {
            Debug.LogWarning("[ECS-TEST] Binder missing", this);
            return;
        }

        if (binder.Entity == Entity.Null)
            return;

        if (!em.Exists(binder.Entity))
            return;

        entity = binder.Entity;
        initialized = true;

        Debug.Log("[ECS-TEST] Bridge initialized → Entity: " + entity.Index, this);
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        TryInitialize();
        if (!initialized || !em.Exists(entity)) return;

        logTimer += Time.fixedDeltaTime;

        // ================= INACTIVE =================
        if (em.HasComponent<EnemyInactive>(entity))
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

            if (logTimer > 2f)
            {
                Debug.Log("[ECS-TEST] Enemy INACTIVE", this);
                logTimer = 0f;
            }

            return;
        }

        var targetData = em.GetComponentData<EnemyTargetPosition>(entity);

        Vector3 pos = rb.position;
        Vector3 target = targetData.Value;

        CurrentTarget = target;

        // ================= DEBUG TARGET =================
        if (logTimer > 2f)
        {
            Debug.Log($"[ECS-TEST] TargetPos: {target} | Pos: {pos}", this);
        }

        // ================= DIRECTION =================
        Vector3 toTarget = target - pos;

        Vector3 flatDir = toTarget;
        flatDir.y = 0;

        float dist = flatDir.magnitude;

        if (logTimer > 2f)
        {
            Debug.Log($"[ECS-TEST] Dist: {dist}", this);
        }

        if (dist < 0.3f)
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x * 0.2f,
                rb.linearVelocity.y,
                rb.linearVelocity.z * 0.2f
            );

            if (logTimer > 2f)
            {
                Debug.Log("[ECS-TEST] Close to target → slowing", this);
                logTimer = 0f;
            }

            return;
        }

        // ================= GROUND =================
        Vector3 groundNormal = Vector3.up;
        bool grounded = false;

        if (Physics.Raycast(
            pos + Vector3.up * 0.5f,
            Vector3.down,
            out RaycastHit hit,
            2f,
            groundMask,
            QueryTriggerInteraction.Ignore))
        {
            groundNormal = hit.normal;
            grounded = true;
        }

        if (!grounded)
        {
            rb.AddForce(Vector3.down * 20f, ForceMode.Acceleration);

            if (logTimer > 2f)
            {
                Debug.LogWarning("[ECS-TEST] NOT GROUNDED!", this);
            }
        }

        // ================= MOVE =================
        Vector3 moveDir = Vector3.ProjectOnPlane(flatDir, groundNormal).normalized;

        // ================= STEP =================
        // ================= STEP / OBSTACLE =================
        Vector3 forward = moveDir;

        bool blocked = Physics.Raycast(
            pos + Vector3.up * 0.2f,
            forward,
            0.6f,
            obstacleMask);

        bool spaceAbove = !Physics.Raycast(
            pos + Vector3.up * 1.0f,
            forward,
            0.6f,
            obstacleMask);

        if (blocked && spaceAbove)
        {
            // вверх
            rb.AddForce(Vector3.up * 5f, ForceMode.VelocityChange);

            // вперёд (ВАЖНО!)
            rb.AddForce(forward * 2f, ForceMode.VelocityChange);

            Debug.Log("[ECS-TEST] JUMP OVER OBSTACLE");
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

        CurrentSpeed = speed;

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

        if (logTimer > 2f)
        {
            Debug.Log($"[ECS-TEST] Velocity: {rb.linearVelocity}", this);
            logTimer = 0f;
        }
    }
}
