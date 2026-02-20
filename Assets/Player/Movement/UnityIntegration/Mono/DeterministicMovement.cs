using UnityEngine;
using FishNet.Object;

[RequireComponent(typeof(CharacterController))]
public class DeterministicMovement : NetworkBehaviour
{
    public float Speed = 6f;
    public float Gravity = -20f;
    public float JumpForce = 7f;

    [SerializeField] private float rotationSharpness = 20f;

    private CharacterController controller;

    private float verticalVelocity;
    private float currentYaw;

    // 👇 добавляем обратно
    public Vector3 Velocity { get; private set; }
    public bool Grounded => controller.isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        currentYaw = transform.eulerAngles.y;
    }

    public void Simulate(MoveCommand cmd)
    {
        if (!IsServer)
            return;

        float dt = NetworkTickSystem.TickDelta;

        // ----- Rotation -----
        currentYaw = Mathf.LerpAngle(
            currentYaw,
            cmd.Yaw,
            1f - Mathf.Exp(-rotationSharpness * dt)
        );

        transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);

        // ----- Horizontal -----
        Vector3 moveDir =
            transform.forward * cmd.Move.y +
            transform.right   * cmd.Move.x;

        moveDir = moveDir.normalized * Speed;

        // ----- Vertical -----
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (cmd.Jump)
                verticalVelocity = JumpForce;
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

        controller.Move(Velocity * dt);
    }
}