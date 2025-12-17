using System;
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        // ===== MOVEMENT =====
        public void SetSpeed(float normalizedSpeed)
        {
            _animator.SetFloat("Speed", normalizedSpeed, 0.15f, Time.deltaTime);
        }

        public void SetGrounded(bool grounded)
        {
            _animator.SetBool("IsGround", grounded);
        }

        public void SetCrouch(bool crouch)
        {
            _animator.SetBool("IsCrouch", crouch);
        }

        public void TriggerJump()
        {
            _animator.SetTrigger("JumpTrigger");
        }

        // ===== WEAPON =====
        public void SetWeaponPose(int pose)
        {
            _animator.SetInteger("WeaponPose", pose);
        }

        public void TriggerThrow()
        {
            _animator.SetTrigger("Throw");
        }

        internal void SetAnimator(Animator animator)
        {
            _animator = animator;
        }
    }
}
