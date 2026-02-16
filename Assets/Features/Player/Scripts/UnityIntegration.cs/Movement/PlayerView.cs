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

        float planarSpeed = new Vector2(
            movement.Velocity.x,
            movement.Velocity.z).magnitude;

        anim.SetSpeed(planarSpeed);
        anim.SetGrounded(movement.Grounded);
    }
}
