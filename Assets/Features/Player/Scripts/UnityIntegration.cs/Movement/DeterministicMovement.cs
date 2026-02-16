using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NetworkBehaviour))]
public class DeterministicMovement : MonoBehaviour
{
    [Header("Movement")]
    public float Speed = 6f;
    public float Gravity = -20f;
    public float JumpForce = 7f;

    [Header("Rotation")]
    private float currentYaw;
    private const float rotationSharpness = 25f;

    [Header("Ground")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundSnapDistance = 0.4f;

    public bool IsServerAuthority;

    public Vector3 Velocity;
    public bool Grounded { get; private set; }

    private CapsuleCollider capsule;
    private NetworkBehaviour networkBehaviour;

    private void Awake()
    {
        capsule = GetComponent<CapsuleCollider>();
        networkBehaviour = GetComponent<NetworkBehaviour>();
    }

    public void Simulate(MoveCommand cmd)
    {
        float dt = NetworkTickSystem.TickDelta;

        // ================= ROTATION =================
        currentYaw = Mathf.LerpAngle(
            currentYaw,
            cmd.Yaw,
            1f - Mathf.Exp(-rotationSharpness * dt)
        );

        transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);

        // ================= GROUND CHECK =================
        bool shouldCheckGround =
            IsServerAuthority ||
            networkBehaviour.IsOwner;

        if (shouldCheckGround)
        {
            Grounded = CheckGround(out RaycastHit hit);

            if (Grounded && Velocity.y < 0f)
                Velocity.y = 0f;
        }

        // ================= HORIZONTAL =================
        Vector3 moveDir =
            (transform.forward * cmd.Move.y +
             transform.right * cmd.Move.x).normalized;

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
        transform.position += Velocity * dt;

        // ================= FINAL SNAP =================
        if (shouldCheckGround)
        {
            if (CheckGround(out RaycastHit hit))
            {
                Grounded = true;

                if (Velocity.y < 0f)
                    Velocity.y = 0f;

                // Snap exactly to ground surface
                float bottomOffset =
                    capsule.height * 0.5f;

                transform.position = new Vector3(
                    transform.position.x,
                    hit.point.y + bottomOffset,
                    transform.position.z
                );
            }
            else
            {
                Grounded = false;
            }
        }
    }

    private bool CheckGround(out RaycastHit hit)
    {
        float radius = capsule.radius * 0.95f;

        Vector3 origin =
            transform.position +
            Vector3.up * (capsule.height * 0.5f - radius);

        return Physics.SphereCast(
            origin,
            radius,
            Vector3.down,
            out hit,
            groundSnapDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }
}
