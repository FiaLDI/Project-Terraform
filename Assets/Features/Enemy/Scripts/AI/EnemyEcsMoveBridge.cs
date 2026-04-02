using FishNet.Object;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public sealed class EnemyEcsMoveBridge : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 8f;

    [Header("Steering")]
    public float orbitStrength = 0.6f;
    public float avoidDistance = 1.5f;
    public float sideAvoidDistance = 1.2f;
    public float separationRadius = 1.5f;

    [Header("Layers")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask groundMask;
    private Vector3 smoothDir;
    private float jumpCooldown;

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

        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null)
            em = world.EntityManager;
    }

    private void TryInit()
    {
        if (initialized) return;

        var binder = GetComponent<EnemyEcsRuntimeBinder>();
        if (binder == null || binder.Entity == Entity.Null) return;

        if (!em.Exists(binder.Entity)) return;

        entity = binder.Entity;
        initialized = true;
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        TryInit();
        if (!initialized) return;

        var targetData = em.GetComponentData<EnemyTargetPosition>(entity);
        var ai = em.GetComponentData<EnemyAI>(entity);

        Vector3 pos = rb.position;
        Vector3 target = targetData.Value;

        // ================= FLAT =================
        Vector3 flatTarget = new Vector3(target.x, pos.y, target.z);
        Vector3 toTarget = flatTarget - pos;

        float dist = toTarget.magnitude;

        if (dist < 0.1f)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        Vector3 forward = toTarget.normalized;

        // ================= ORBIT =================
        Vector3 orbit = Vector3.zero;

        if (dist < ai.AttackRange * 1.2f)
        {
            Vector3 right = new Vector3(-forward.z, 0, forward.x);
            orbit = right * orbitStrength;
        }

        // ================= AVOID (3 rays) =================
        Vector3 avoid = Vector3.zero;

        Vector3 origin = pos + Vector3.up * 0.5f;

        Vector3 left = Quaternion.Euler(0, -30, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, 30, 0) * forward;

        if (Physics.Raycast(origin, forward, out RaycastHit hitF, avoidDistance, obstacleMask))
            avoid += hitF.normal;

        if (Physics.Raycast(origin, left, out RaycastHit hitL, sideAvoidDistance, obstacleMask))
            avoid += hitL.normal;

        if (Physics.Raycast(origin, rightDir, out RaycastHit hitR, sideAvoidDistance, obstacleMask))
            avoid += hitR.normal;

        // ================= SEPARATION =================
        Vector3 separation = Vector3.zero;

        Collider[] neighbors = Physics.OverlapSphere(pos, separationRadius);

        foreach (var col in neighbors)
        {
            if (col.attachedRigidbody == rb) continue;
            if (!col.CompareTag("Enemy")) continue;

            Vector3 diff = pos - col.transform.position;
            diff.y = 0;

            float d = diff.magnitude;

            if (d > 0.001f)
                separation += diff.normalized / d;
        }

        // ================= FINAL DIR =================
        Vector3 desiredDir =
            forward * 1.0f +
            orbit * 0.8f +
            avoid * 2.0f +
            separation * 1.5f;

        desiredDir.y = 0;

        if (desiredDir.sqrMagnitude > 0.001f)
            desiredDir.Normalize();
        else
            desiredDir = Vector3.zero;

        // 🔥 сглаживание
        smoothDir = Vector3.Lerp(
            smoothDir,
            desiredDir,
            8f * Time.fixedDeltaTime
        );

        Vector3 finalDir = smoothDir;

        finalDir.y = 0;
        if (finalDir.sqrMagnitude > 0.001f)
            finalDir.Normalize();
        else
            finalDir = Vector3.zero;

        // ================= GROUND =================
        Vector3 groundNormal = Vector3.up;

        if (Physics.Raycast(pos + Vector3.up * 0.5f, Vector3.down, out RaycastHit groundHit, 2f, groundMask))
        {
            groundNormal = groundHit.normal;
        }

        finalDir = Vector3.ProjectOnPlane(finalDir, groundNormal).normalized;

        // ================= MOVE =================
        float speed = moveSpeed;

        if (dist < 1.5f)
            speed *= dist / 1.5f;

        Vector3 vel = finalDir * speed;

        Vector3 currentVel = rb.linearVelocity;

        Vector3 targetVel = new Vector3(
            vel.x,
            currentVel.y,
            vel.z
        );

        rb.linearVelocity = Vector3.Lerp(
            currentVel,
            targetVel,
            10f * Time.fixedDeltaTime
        );

        bool obstacleAhead =
            Physics.Raycast(origin, forward, out RaycastHit jumpHit, 0.8f, obstacleMask);

        bool grounded =
            Physics.Raycast(pos + Vector3.up * 0.2f, Vector3.down, 0.4f, groundMask);

        jumpCooldown -= Time.fixedDeltaTime;

        if (obstacleAhead && grounded && jumpCooldown <= 0f)
        {
            rb.AddForce(Vector3.up * 5f, ForceMode.VelocityChange);
            jumpCooldown = 0.5f; // 🔥 задержка между прыжками
        }

        // ================= ROTATION =================
        if (finalDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(finalDir);

            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation,
                targetRot,
                rotationSpeed * Time.fixedDeltaTime
            ));
        }

        // ================= SYNC ECS =================
        var t = em.GetComponentData<LocalTransform>(entity);
        t.Position = rb.position;
        em.SetComponentData(entity, t);
    }
}
