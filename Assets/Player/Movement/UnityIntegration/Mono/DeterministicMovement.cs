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
    public float JumpForce = 7f;

    [SerializeField] private float crouchHeight = 2f;
    [SerializeField] private float normalHeight = 3f;

    [SerializeField] private float rotationSharpness = 20f;
    [SerializeField] private float jumpHeight = 1.2f;

    private CharacterController controller;
    private bool isCrouching;
    private float verticalVelocity;
    private float currentYaw;
    private bool wasGrounded;

    public Vector3 Velocity { get; private set; }
    public bool Grounded => controller.isGrounded;
    public bool IsCrouching => isCrouching;
    private MovementStatsAdapter movementStats;

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

        currentYaw = cmd.Yaw;
        transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);

        // ================= SPEED =================
        float speed = 5f;

        if (movementStats != null && movementStats.IsReady)
        {
            speed = movementStats.GetSpeed(cmd.Sprint, cmd.Crouch);
        }

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

        // ================= HORIZONTAL =================
        Vector3 moveDir =
            transform.forward * cmd.Move.y +
            transform.right * cmd.Move.x;

        moveDir = moveDir.normalized * speed;

        // ================= VERTICAL =================\
        JumpedThisTick = false;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (cmd.Jump && !cmd.Crouch)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * Gravity);
                JumpedThisTick = true;
            }
        }
        else
        {
            verticalVelocity += Gravity * dt;
        }

        wasGrounded = controller.isGrounded;

        Velocity = new Vector3(
            moveDir.x,
            verticalVelocity,
            moveDir.z
        );

        controller.Move(Velocity * dt);
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

    private void TryResolveStats()
    {
        if (movementStats != null)
            return;

        var stats = GetComponent<StatsFacadeAdapter>();
        if (stats != null)
            movementStats = stats.MovementStats;
    }
}