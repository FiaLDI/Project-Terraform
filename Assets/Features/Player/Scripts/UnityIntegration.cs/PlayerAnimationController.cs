using UnityEngine;

namespace Features.Player.UnityIntegration
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        private Animator _animator;

        private bool IsReady =>
            _animator != null &&
            _animator.runtimeAnimatorController != null;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        // ===== MOVEMENT =====
        public void SetSpeed(float normalizedSpeed)
        {
            if (!IsReady) return;
            _animator.SetFloat("Speed", normalizedSpeed, 0.15f, Time.deltaTime);
        }

        public void SetGrounded(bool grounded)
        {
            if (!IsReady) return;
            _animator.SetBool("IsGround", grounded);
        }

        public void SetCrouch(bool crouch)
        {
            if (!IsReady) return;
            _animator.SetBool("IsCrouch", crouch);
        }

        public void TriggerJump()
        {
            if (!IsReady) return;
            _animator.SetTrigger("JumpTrigger");
        }

        // ===== WEAPON =====
        public void SetWeaponPose(int pose)
        {
            if (!IsReady) return;
            _animator.SetInteger("WeaponPose", pose);
        }

        public void TriggerThrow()
        {
            if (!IsReady) return;
            _animator.SetTrigger("Throw");
        }

        internal void SetAnimator(Animator animator)
        {
            _animator = animator;
        }

        public void ForceRefreshLocomotion()
        {
            if (!IsReady) return;
            _animator.Play("Locomotion", 0, 0f);
        }

    }
}
