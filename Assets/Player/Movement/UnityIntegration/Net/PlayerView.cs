using Features.Player.UnityIntegration;
using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(DeterministicMovement))]
[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerView : NetworkBehaviour
{
    private DeterministicMovement movement;
    private PlayerAnimationController anim;

    [SerializeField] private Transform visualRoot;
    private Vector3 smoothVelocity;

    private RemoteInterpolation remote;
    private MovementInputHandler inputHandler;

    private void Awake()
    {
        movement = GetComponent<DeterministicMovement>();
        anim = GetComponent<PlayerAnimationController>();
        remote = GetComponentInChildren<RemoteInterpolation>();
    }

    private void Start()
    {
        if (LocalPlayerController.I != null)
            inputHandler = LocalPlayerController.I.GetComponent<MovementInputHandler>();
    }

    private void Update()
    {
        if (anim == null)
            return;

        if (inputHandler == null && LocalPlayerController.I != null)
            inputHandler = LocalPlayerController.I.GetComponent<MovementInputHandler>();

        float yaw = visualRoot != null ? visualRoot.localEulerAngles.y : transform.eulerAngles.y;

        if (IsOwner)
        {
            if (inputHandler != null)
                yaw = inputHandler.CurrentState.Yaw;
        }
        else if (remote != null)
        {
            yaw = remote.GetInterpolatedYaw();
        }

        if (visualRoot != null)
        {
            if (IsOwner)
            {
                visualRoot.localRotation = Quaternion.Euler(0f, yaw, 0f);
            }
            else
            {
                float smoothYaw = Mathf.LerpAngle(
                    visualRoot.localEulerAngles.y,
                    yaw,
                    1f - Mathf.Exp(-20f * Time.deltaTime)
                );

                visualRoot.localRotation = Quaternion.Euler(0f, smoothYaw, 0f);
            }
        }

        Vector3 sourceVelocity = movement.Velocity;
        bool grounded = movement.Grounded;
        bool crouching = movement.IsCrouching;
        int weaponPose = 0;

        if (!IsOwner && remote != null)
        {
            sourceVelocity = remote.GetInterpolatedVelocity();
            grounded = remote.IsGrounded();
            crouching = remote.IsCrouching();
            weaponPose = remote.GetWeaponPose();
        }

        smoothVelocity = Vector3.Lerp(
            smoothVelocity,
            sourceVelocity,
            1f - Mathf.Exp(-15f * Time.deltaTime)
        );

        Vector3 planarVelocity = new Vector3(smoothVelocity.x, 0f, smoothVelocity.z);
        Vector3 localVelocity = Quaternion.Inverse(Quaternion.Euler(0f, yaw, 0f)) * planarVelocity;
        float maxSpeed = Mathf.Max(0.01f, movement.CurrentMaxSpeed);
        float normalizedSpeed = Mathf.Clamp01(new Vector2(localVelocity.z, localVelocity.x).magnitude / maxSpeed);

        anim.SetSpeed(normalizedSpeed);
        anim.SetGrounded(grounded);
        anim.SetCrouch(crouching);

        if (!IsOwner)
            anim.SetWeaponPose(weaponPose);

        if (movement.JumpedThisTick)
            anim.TriggerJump();
    }
}
