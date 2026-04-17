using UnityEngine;
using FishNet.Object;
using Features.Stats.Adapter;
using FishNet;

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

    public Vector3 Velocity { get; private set; }
    public bool Grounded => controller.isGrounded;
    public bool IsCrouching => isCrouching;

    public float VerticalVelocityInternal => verticalVelocity;

    private MovementStatsAdapter movementStats;

    private float jumpBufferTimer;
    private const float JumpBufferTime = 0.15f;

    private float coyoteTimer;
    private const float CoyoteTime = 0.15f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        var stats = GetComponent<StatsFacadeAdapter>();
        if (stats != null)
            movementStats = stats.MovementStats;
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

        float dt = (float)InstanceFinder.TimeManager.TickDelta;

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

        // ================= SPEED =================

        float speed = 5f;

        if (movementStats != null && movementStats.IsReady)
            speed = movementStats.GetSpeed(cmd.Sprint, cmd.Crouch);

        CurrentMaxSpeed = speed;

        // ================= CROUCH =================

        SetCrouchState(cmd.Crouch);

        // ================= MOVEMENT (через YAW ИЗ CMD) =================

        float yaw = cmd.Yaw;
        float rad = yaw * Mathf.Deg2Rad;

        Vector3 forward = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
        Vector3 right   = new Vector3(forward.z, 0f, -forward.x);

        Vector3 moveDir =
            forward * cmd.Move.y +
            right   * cmd.Move.x;

        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        moveDir *= speed;

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

        controller.Move((horizontal + vertical) * dt);
    }

    // ================= STATE =================

    public void Teleport(Vector3 position, float verticalVel)
    {
        controller.enabled = false;

        transform.position = position;
        verticalVelocity = verticalVel;

        controller.enabled = true;
    }

    public void ApplyState(PlayerState state)
    {
        controller.enabled = false;

        SetCrouchState(state.Crouch);
        transform.position = state.Position;
        Velocity = state.Velocity;
        verticalVelocity = state.VerticalVelocity;
        CurrentMaxSpeed = new Vector3(state.Velocity.x, 0f, state.Velocity.z).magnitude;
        JumpedThisTick = false;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;

        controller.enabled = true;

        controller.Move(Vector3.zero);
    }

    public void ApplyCorrection(Vector3 correction)
    {
        if (correction.sqrMagnitude <= 0f)
            return;

        bool wasEnabled = controller.enabled;
        controller.enabled = false;
        transform.position += correction;
        controller.enabled = wasEnabled;

        if (controller.enabled)
            controller.Move(Vector3.zero);
    }

    private void TryResolveStats()
    {
        if (movementStats != null)
            return;

        var stats = GetComponent<StatsFacadeAdapter>();
        if (stats != null)
            movementStats = stats.MovementStats;
    }

    private void SetCrouchState(bool crouch)
    {
        if (isCrouching == crouch)
            return;

        isCrouching = crouch;

        float height = isCrouching ? crouchHeight : normalHeight;
        controller.height = height;
        controller.center = new Vector3(0f, height / 2f, 0f);
    }
}
