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

    [Header("Jump")]
    public float jumpForce = 6f;
    public float highJumpForce = 8.5f;
    public float jumpForwardBoost = 1.25f;
    public float jumpCooldownTime = 0.75f;
    public float jumpObstacleHeight = 0.9f;
    public float highJumpObstacleHeight = 1.6f;
    public float groundCheckDistance = 0.35f;

    [Header("Detour")]
    public float detourDistance = 2.5f;
    public float detourProbeDistance = 2f;
    public float detourAngle = 55f;

    [Header("Steering")]
    public float orbitStrength = 0.6f;
    public float avoidDistance = 1.5f;
    public float sideAvoidDistance = 1.2f;
    public float separationRadius = 1.5f;

    [Header("Layers")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask enemyMask;

    private readonly Collider[] separationBuffer = new Collider[32];

    private Vector3 smoothDir;
    private float jumpCooldown;

    private Entity entity;
    private EntityManager em;
    private Rigidbody rb;
    private bool initialized;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (obstacleMask.value == 0)
            obstacleMask = Physics.DefaultRaycastLayers;

        if (groundMask.value == 0)
            groundMask = Physics.DefaultRaycastLayers;

        if (enemyMask.value == 0)
            enemyMask = Physics.DefaultRaycastLayers;

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        var world = Unity.Entities.World.DefaultGameObjectInjectionWorld;
        if (world != null)
            em = world.EntityManager;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsServer && rb != null)
            rb.isKinematic = true; // ❗ клиент не симулирует физику
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

        if (jumpCooldown > 0f)
            jumpCooldown -= Time.fixedDeltaTime;

        TryInit();
        if (!initialized) return;

        if (!em.Exists(entity))
        {
            enabled = false;
            return;
        }

        var targetData = em.GetComponentData<EnemyTargetPosition>(entity);
        var ai = em.GetComponentData<EnemyAI>(entity);
        var enemyState = em.GetComponentData<EnemyState>(entity);
        var steering = em.GetComponentData<EnemySteeringData>(entity);

        Vector3 pos = rb.position;
        Vector3 target = targetData.Value;
        Vector3 moveTarget = target;
        float avoidDistanceValue = steering.avoidDistance > 0f ? steering.avoidDistance : avoidDistance;
        float sideAvoidDistanceValue = steering.sideAvoidDistance > 0f ? steering.sideAvoidDistance : sideAvoidDistance;
        float separationRadiusValue = steering.separationRadius > 0f ? steering.separationRadius : separationRadius;
        float rotationSpeedValue = steering.rotationSpeed > 0f ? steering.rotationSpeed : rotationSpeed;
        float directionSmoothingValue = steering.directionSmoothing > 0f ? steering.directionSmoothing : 8f;

        Vector3 flatTarget = new Vector3(moveTarget.x, pos.y, moveTarget.z);
        Vector3 toTarget = flatTarget - pos;

        float dist = toTarget.magnitude;

        if (dist < 0.1f)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        Vector3 forward = toTarget.normalized;
        float attackEnterDistance = Mathf.Max(0f, ai.AttackRange - ai.AttackEnterOffset);
        bool isCombatState =
            enemyState.Value == EnemyAIState.Chase ||
            enemyState.Value == EnemyAIState.Attack;
        bool forceCloseApproach = isCombatState && dist <= attackEnterDistance + 0.35f;

        // ===== ORBIT =====
        Vector3 orbit = Vector3.zero;
        bool allowOrbit =
            steering.enableOrbit &&
            enemyState.Value == EnemyAIState.Attack &&
            dist <= ai.AttackRange &&
            !forceCloseApproach;

        if (allowOrbit)
        {
            Vector3 right = new Vector3(-forward.z, 0, forward.x);
            orbit = right * steering.orbitStrength;
        }

        // ===== AVOID =====
        Vector3 avoid = Vector3.zero;
        Vector3 origin = pos + Vector3.up * 0.5f;
        bool forwardBlocked = false;

        Vector3 left = Quaternion.Euler(0, -30, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, 30, 0) * forward;

        if (Physics.Raycast(origin, forward, out var hitF, avoidDistanceValue, obstacleMask))
        {
            avoid += hitF.normal;
            forwardBlocked = true;
        }

        if (Physics.Raycast(origin, left, out var hitL, sideAvoidDistanceValue, obstacleMask))
            avoid += hitL.normal;

        if (Physics.Raycast(origin, rightDir, out var hitR, sideAvoidDistanceValue, obstacleMask))
            avoid += hitR.normal;

        // ===== SEPARATION =====
        Vector3 separation = Vector3.zero;

        int neighborCount = Physics.OverlapSphereNonAlloc(
            pos,
            separationRadiusValue,
            separationBuffer,
            enemyMask,
            QueryTriggerInteraction.Ignore
        );

        for (int i = 0; i < neighborCount; i++)
        {
            var col = separationBuffer[i];
            if (col == null) continue;
            if (col.attachedRigidbody == rb) continue;
            if (!col.CompareTag("Enemy")) continue;

            Vector3 diff = pos - col.transform.position;
            diff.y = 0;

            float d = diff.magnitude;
            if (d > 0.001f)
                separation += diff.normalized / d;
        }

        if (!steering.enableAvoidance)
            avoid = Vector3.zero;

        if (!steering.enableSeparation)
            separation = Vector3.zero;

        if (forceCloseApproach)
        {
            orbit = Vector3.zero;
            avoid *= 0.5f;
            separation *= 0.25f;
        }

        // ===== FINAL DIR =====
        Vector3 desiredDir =
            forward * steering.seekWeight +
            orbit * steering.orbitWeight +
            avoid * steering.avoidWeight +
            separation * steering.separationWeight;

        desiredDir.y = 0;

        if (desiredDir.sqrMagnitude > 0.001f)
            desiredDir.Normalize();

        smoothDir = Vector3.Lerp(
            smoothDir,
            desiredDir,
            directionSmoothingValue * Time.fixedDeltaTime
        );

        Vector3 finalDir = smoothDir.normalized;

        bool canDoNormalJump =
            forwardBlocked &&
            jumpCooldown <= 0f &&
            IsGrounded(pos) &&
            HasJumpClearance(pos, finalDir, avoidDistanceValue);

        bool canDoHighJump =
            forwardBlocked &&
            !canDoNormalJump &&
            jumpCooldown <= 0f &&
            IsGrounded(pos) &&
            HasHighJumpClearance(pos, finalDir, avoidDistanceValue);

        if (forwardBlocked && !canDoNormalJump && !canDoHighJump)
        {
            moveTarget = GetDetourTarget(pos, target, forward);
            flatTarget = new Vector3(moveTarget.x, pos.y, moveTarget.z);
            toTarget = flatTarget - pos;
            dist = toTarget.magnitude;

            if (dist >= 0.1f)
            {
                forward = toTarget.normalized;

                Vector3 detourDesiredDir = forward +
                                           orbit * steering.orbitWeight +
                                           avoid * steering.avoidWeight +
                                           separation * steering.separationWeight;

                detourDesiredDir.y = 0f;

                if (detourDesiredDir.sqrMagnitude > 0.001f)
                    detourDesiredDir.Normalize();

                smoothDir = Vector3.Lerp(
                    smoothDir,
                    detourDesiredDir,
                    directionSmoothingValue * Time.fixedDeltaTime
                );

                finalDir = smoothDir.normalized;
            }
        }

        // ===== MOVE =====
        float speed = ai.MoveSpeed > 0f ? ai.MoveSpeed : moveSpeed;

        if (dist < 1.5f)
            speed *= dist / 1.5f;

        Vector3 vel = finalDir * speed;

        if (canDoNormalJump || canDoHighJump)
        {
            jumpCooldown = jumpCooldownTime;
            vel += finalDir * jumpForwardBoost;
        }

        rb.linearVelocity = new Vector3(
            vel.x,
            canDoHighJump
                ? Mathf.Max(rb.linearVelocity.y, highJumpForce)
                : canDoNormalJump
                    ? Mathf.Max(rb.linearVelocity.y, jumpForce)
                    : rb.linearVelocity.y,
            vel.z
        );

        // ===== ROTATION =====
        if (finalDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(finalDir);

            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation,
                targetRot,
                rotationSpeedValue * Time.fixedDeltaTime
            ));
        }

        // ===== ECS SYNC =====
        var t = em.GetComponentData<LocalTransform>(entity);
        t.Position = rb.position;
        t.Rotation = rb.rotation;
        em.SetComponentData(entity, t);
    }

    private bool IsGrounded(Vector3 pos)
    {
        Vector3 origin = pos + Vector3.up * 0.1f;
        return Physics.Raycast(
            origin,
            Vector3.down,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private bool HasJumpClearance(Vector3 pos, Vector3 direction, float avoidDistanceValue)
    {
        if (direction.sqrMagnitude < 0.001f)
            return false;

        Vector3 lowOrigin = pos + Vector3.up * 0.2f;
        Vector3 highOrigin = pos + Vector3.up * jumpObstacleHeight;

        bool lowBlocked = Physics.Raycast(
            lowOrigin,
            direction,
            avoidDistanceValue,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        if (!lowBlocked)
            return false;

        bool highBlocked = Physics.Raycast(
            highOrigin,
            direction,
            avoidDistanceValue,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        return !highBlocked;
    }

    private bool HasHighJumpClearance(Vector3 pos, Vector3 direction, float avoidDistanceValue)
    {
        if (direction.sqrMagnitude < 0.001f)
            return false;

        Vector3 lowOrigin = pos + Vector3.up * (jumpObstacleHeight * 0.5f);
        Vector3 highOrigin = pos + Vector3.up * highJumpObstacleHeight;

        bool lowBlocked = Physics.Raycast(
            lowOrigin,
            direction,
            avoidDistanceValue,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        if (!lowBlocked)
            return false;

        bool highBlocked = Physics.Raycast(
            highOrigin,
            direction,
            avoidDistanceValue,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        return !highBlocked;
    }

    private Vector3 GetDetourTarget(Vector3 pos, Vector3 originalTarget, Vector3 forward)
    {
        if (forward.sqrMagnitude < 0.001f)
            return originalTarget;

        Vector3 leftDir = Quaternion.Euler(0f, -detourAngle, 0f) * forward;
        Vector3 rightDir = Quaternion.Euler(0f, detourAngle, 0f) * forward;

        bool leftBlocked = Physics.Raycast(
            pos + Vector3.up * 0.5f,
            leftDir,
            detourProbeDistance,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        bool rightBlocked = Physics.Raycast(
            pos + Vector3.up * 0.5f,
            rightDir,
            detourProbeDistance,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        Vector3 detourDir;
        if (leftBlocked && !rightBlocked)
            detourDir = rightDir;
        else if (rightBlocked && !leftBlocked)
            detourDir = leftDir;
        else
            detourDir = leftDir;

        detourDir.y = 0f;
        detourDir.Normalize();

        return pos + detourDir * detourDistance;
    }
}
