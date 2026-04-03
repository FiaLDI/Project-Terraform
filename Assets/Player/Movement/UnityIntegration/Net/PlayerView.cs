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

    private void Awake()
    {
        movement = GetComponent<DeterministicMovement>();
        anim = GetComponent<PlayerAnimationController>();
        net = GetComponent<PlayerNetworkController>();
    }

    private void Update()
    {
        if (!IsOwner)
            return;
        if (anim == null)
            return;

        float t = Mathf.Clamp01(Time.deltaTime / NetworkTickSystem.TickDelta);

        Vector3 from = net.GetPreviousPosition();
        Vector3 to = net.GetCurrentPosition();

        renderPosition = Vector3.Lerp(from, to, t);

        Quaternion fromRot = net.GetPreviousRotation();
        Quaternion toRot = net.GetCurrentRotation();

        renderRotation = Quaternion.Slerp(fromRot, toRot, t);

        visualRoot.position = renderPosition;
        visualRoot.rotation = renderRotation;

        // ================= �������� =================

        smoothVelocity = Vector3.Lerp(
            smoothVelocity,
            movement.Velocity,
            1f - Mathf.Exp(-15f * Time.deltaTime)
        );

        Vector3 localVel =
            visualRoot.InverseTransformDirection(smoothVelocity);

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
