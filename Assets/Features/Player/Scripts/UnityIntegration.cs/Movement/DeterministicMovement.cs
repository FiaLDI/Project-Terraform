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
    [SerializeField] private float groundCheckDistance = 0.2f;

    public bool IsServerAuthority;

    public Vector3 Velocity;
    public bool Grounded { get; private set; }

    private CapsuleCollider capsule;

    private void Awake()
    {
        capsule = GetComponent<CapsuleCollider>();
    }

    public void Simulate(MoveCommand cmd)
    {
        float dt = NetworkTickSystem.TickDelta;

        // ROTATION
        currentYaw = Mathf.LerpAngle(
            currentYaw,
            cmd.Yaw,
            1f - Mathf.Exp(-rotationSharpness * dt)
        );

        transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);

        // GROUND CHECK
        Grounded = CheckGround();

        if (Grounded && Velocity.y < 0f)
            Velocity.y = 0f;

        // HORIZONTAL
        Vector3 moveDir =
            (transform.forward * cmd.Move.y +
             transform.right * cmd.Move.x).normalized;

        Vector3 horizontal = moveDir * Speed;

        Velocity.x = horizontal.x;
        Velocity.z = horizontal.z;

        // JUMP
        if (Grounded && cmd.Jump)
        {
            Velocity.y = JumpForce;
            Grounded = false;
        }

        // GRAVITY
        if (!Grounded)
            Velocity.y += Gravity * dt;

        // APPLY
        transform.position += Velocity * dt;
    }

    private bool CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        return Physics.Raycast(
            origin,
            Vector3.down,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }
}
