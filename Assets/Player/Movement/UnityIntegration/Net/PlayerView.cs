using Features.Player.UnityIntegration;
using FishNet.Object;
using UnityEngine;

[RequireComponent(typeof(DeterministicMovement))]
[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerView : NetworkBehaviour
{
    private DeterministicMovement movement;
    private PlayerAnimationController anim;

    private void Awake()
    {
        movement = GetComponent<DeterministicMovement>();
        anim = GetComponent<PlayerAnimationController>();
    }

    private void Update()
    {
        if (anim == null)
            return;

        Vector3 localVel =
            transform.InverseTransformDirection(movement.Velocity);

        float forward = localVel.z;
        float right   = localVel.x;

        float normalizedSpeed =
            Mathf.Clamp01(new Vector2(forward, right).magnitude / movement.Speed);

        anim.SetSpeed(normalizedSpeed);
        anim.SetGrounded(movement.Grounded);
    }
}
