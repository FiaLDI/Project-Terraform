using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class DeterministicMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float Speed = 6f;
    public float Gravity = -20f;
    public float JumpForce = 7f;

    [Header("Rotation")]
    private float currentYaw;
    private float targetYaw;
    private const float rotationSharpness = 25f;

    [Header("Ground")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundSnapDistance = 0.4f;
    public Vector3 Velocity;

    public bool Grounded { get; private set; }

    private CapsuleCollider capsule;

    private void Awake()
    {
        capsule = GetComponent<CapsuleCollider>();
    }

    public void Simulate(MoveCommand cmd)
    {
        if (!IsServer && !IsOwner)
            return;
        float dt = NetworkTickSystem.TickDelta;

        // ================= ROTATION (smooth like CS2) =================
        targetYaw = cmd.Yaw;

        currentYaw = Mathf.LerpAngle(
            currentYaw,
            targetYaw,
            1f - Mathf.Exp(-rotationSharpness * dt)
        );

        transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);

        // ================= GROUND CHECK BEFORE MOVE =================
        bool wasGrounded = Grounded;
        Grounded = CheckGround(out RaycastHit hit);

        if (Grounded && Velocity.y < 0f)
            Velocity.y = 0f;

        // ================= HORIZONTAL =================
        Vector3 forward = transform.forward;
        Vector3 right   = transform.right;

        Vector3 moveDir = (forward * cmd.Move.y + right * cmd.Move.x).normalized;
        Vector3 horizontal = moveDir * Speed;

        Velocity.x = horizontal.x;
        Velocity.z = horizontal.z;

        // ================= JUMP =================
        if (Grounded && cmd.Jump)
        {
            Velocity.y = JumpForce;
            Grounded = false;
        }

        // ================= GRAVITY =================
        if (!Grounded)
            Velocity.y += Gravity * dt;

        // ================= APPLY MOVE =================
        Vector3 newPos = transform.position + Velocity * dt;
        transform.position = newPos;

        // ================= SNAP TO GROUND AFTER MOVE =================
        if (CheckGround(out hit))
        {
            Grounded = true;

            if (Velocity.y < 0f)
                Velocity.y = 0f;

            transform.position = new Vector3(
                transform.position.x,
                hit.point.y,
                transform.position.z
            );
        }
        else
        {
            Grounded = false;
        }
    }

    // ==============================================================
    // GROUND CHECK
    // ==============================================================

    private bool CheckGround(out RaycastHit hit)
    {
        float radius = capsule.radius * 0.95f;
        float castDistance = groundSnapDistance;

        // центр нижней сферы капсулы
        Vector3 origin =
            transform.position +
            Vector3.up * radius;

        return Physics.SphereCast(
            origin,
            radius,
            Vector3.down,
            out hit,
            castDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }
}
