using UnityEngine;
using FishNet.Object;
using Features.Stats.Adapter;

[RequireComponent(typeof(CharacterController))]
public class DeterministicMovement : NetworkBehaviour
{
    public float CurrentMaxSpeed { get; private set; }
    public bool JumpedThisTick { get; private set; }
    public bool IsFrozen { get; set; }

    public float Gravity = -40f;

    [SerializeField] private float jumpHeight = 1.2f;

    [SerializeField] private float crouchHeight = 2f;
    [SerializeField] private float normalHeight = 3f;

    private CharacterController controller;
    private bool isCrouching;

    private float verticalVelocity;
    private float currentYaw;

    public Vector3 Velocity { get; private set; }
    public bool Grounded => controller.isGrounded;
    public bool IsCrouching => isCrouching;

    public float VerticalVelocityInternal => verticalVelocity;
    public float CurrentYawInternal => currentYaw;

    private MovementStatsAdapter movementStats;

    // 🔥 JUMP BUFFER
    private float jumpBufferTimer;
    private const float JumpBufferTime = 0.15f;

    // 🔥 COYOTE TIME
    private float coyoteTimer;
    private const float CoyoteTime = 0.15f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        var stats = GetComponent<StatsFacadeAdapter>();
        if (stats != null)
            movementStats = stats.MovementStats;

        currentYaw = transform.eulerAngles.y;
    }

    public void Simulate(MoveCommand cmd)
    {
        if (IsFrozen)
        {
            Velocity = Vector3.zero;
            verticalVelocity = 0f;
            return;
        }

        TryResolveStats();

        float dt = NetworkTickSystem.TickDelta;

        // ================= INPUT BUFFER =================

        jumpBufferTimer -= dt;
        if (cmd.Jump)
            jumpBufferTimer = JumpBufferTime;

        if (jumpBufferTimer < 0f)
            jumpBufferTimer = 0f;

        // ================= COYOTE =================

        if (controller.isGrounded)
            coyoteTimer = CoyoteTime;
        else
            coyoteTimer -= dt;

        if (coyoteTimer < 0f)
            coyoteTimer = 0f;

        // ================= ROTATION =================

        currentYaw = cmd.Yaw;
        // transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);

        // ================= SPEED =================

        float speed = 5f;

        if (movementStats != null && movementStats.IsReady)
            speed = movementStats.GetSpeed(cmd.Sprint, cmd.Crouch);

        CurrentMaxSpeed = speed;

        // ================= CROUCH =================

        if (cmd.Crouch && !isCrouching)
        {
            isCrouching = true;
            controller.height = crouchHeight;
            controller.center = new Vector3(0, crouchHeight / 2f, 0);
        }
        else if (!cmd.Crouch && isCrouching)
        {
            isCrouching = false;
            controller.height = normalHeight;
            controller.center = new Vector3(0, normalHeight / 2f, 0);
        }

        Quaternion lookRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

        Vector3 forward = lookRot * Vector3.forward;
        Vector3 right   = lookRot * Vector3.right;

        Vector3 moveDir =
            forward * cmd.Move.y +
            right   * cmd.Move.x;

        moveDir = moveDir.normalized * speed;

        // ================= JUMP =================

        JumpedThisTick = false;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;
        }

        if (jumpBufferTimer > 0f && coyoteTimer > 0f && !cmd.Crouch)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * Gravity);
            JumpedThisTick = true;

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }
        else
        {
            verticalVelocity += Gravity * dt;
        }

        Velocity = new Vector3(
            moveDir.x,
            verticalVelocity,
            moveDir.z
        );

        Vector3 horizontal = new Vector3(moveDir.x, 0, moveDir.z);
        Vector3 vertical   = new Vector3(0, verticalVelocity, 0);

        controller.Move(horizontal * dt);
        controller.Move(vertical * dt);
    }

    public void Teleport(Vector3 position, float yaw, float verticalVel)
    {
        controller.enabled = false;

        transform.position = position;
        currentYaw = yaw;
        verticalVelocity = verticalVel;

        transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);

        controller.enabled = true;
    }

    public void ApplyState(PlayerState state)
    {
        controller.enabled = false;

        transform.position = state.Position;

        currentYaw = state.InternalYaw;
        verticalVelocity = state.VerticalVelocity;

        transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);

        controller.enabled = true;

        controller.Move(Vector3.zero);
    }

    public void ApplyCorrection(Vector3 correction)
    {
        Velocity += correction * 5f;
    }

    private void TryResolveStats()
    {
        if (movementStats != null)
            return;

        var stats = GetComponent<StatsFacadeAdapter>();
        if (stats != null)
            movementStats = stats.MovementStats;
    }
}
