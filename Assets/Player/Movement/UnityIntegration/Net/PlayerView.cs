using Features.Player.UnityIntegration;
using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(DeterministicMovement))]
[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerView : NetworkBehaviour
{
    private DeterministicMovement movement;
    private PlayerAnimationController anim;
    private PlayerNetworkController net;

    [SerializeField] private Transform visualRoot;
    private Vector3 visualVelocity;
    private Vector3 smoothVelocity;

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private Vector3 renderPosition;
    private Quaternion renderRotation;
    private RemoteInterpolation remote;
    private MovementInputHandler inputHandler;

    private void Awake()
    {
        movement = GetComponent<DeterministicMovement>();
        anim = GetComponent<PlayerAnimationController>();
        net = GetComponent<PlayerNetworkController>();
        remote = GetComponentInChildren<RemoteInterpolation>();
    }

    private void Start()
    {
        inputHandler = LocalPlayerController.I.GetComponent<MovementInputHandler>();
    }

    private void Update()
    {
        if (anim == null)
            return;

        if (inputHandler == null) {
            inputHandler = LocalPlayerController.I.GetComponent<MovementInputHandler>();
            if (inputHandler != null) return;
        }

        // ================= ВИЗУАЛ ПОВОРОТА =================

        float yaw;

        if (IsOwner)
        {
            yaw = inputHandler.CurrentState.Yaw;
        }
        else
        {
            yaw = remote.GetInterpolatedYaw();
        }

        float smoothYaw = Mathf.LerpAngle(
            visualRoot.localEulerAngles.y,
            yaw,
            1f - Mathf.Exp(-20f * Time.deltaTime)
        );

        visualRoot.localRotation = Quaternion.Euler(0f, smoothYaw, 0f);

        // ================= АНИМАЦИЯ =================

        smoothVelocity = Vector3.Lerp(
            smoothVelocity,
            movement.Velocity,
            1f - Mathf.Exp(-15f * Time.deltaTime)
        );

        Vector3 localVel =
            transform.InverseTransformDirection(smoothVelocity);

        float forward = localVel.z;
        float right = localVel.x;

        float maxSpeed = Mathf.Max(0.01f, movement.CurrentMaxSpeed);

        float normalizedSpeed =
            Mathf.Clamp01(new Vector2(forward, right).magnitude / maxSpeed);

        anim.SetSpeed(normalizedSpeed);
        anim.SetGrounded(movement.Grounded);
        anim.SetCrouch(movement.IsCrouching);

        if (movement.JumpedThisTick)
            anim.TriggerJump();
    }
}
