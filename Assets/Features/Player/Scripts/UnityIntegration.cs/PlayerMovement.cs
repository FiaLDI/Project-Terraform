using UnityEngine;
using FishNet.Object;
using Features.Stats.Adapter;

namespace Features.Player.UnityIntegration
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform movementReference;
        private PlayerAnimationController anim;

        [Header("Fallback Speeds")]
        [SerializeField] private float fallbackBaseSpeed = 5f;
        [SerializeField] private float fallbackWalkSpeed = 2f;
        [SerializeField] private float fallbackSprintSpeed = 8f;
        [SerializeField] private float fallbackCrouchSpeed = 1.5f;

        [Header("Jump & Gravity")]
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpForce = 5f;

        [Header("Crouch")]
        [SerializeField] private float standHeight = 2f;
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float crouchTransitionSpeed = 10f;

        [Header("Air Control")]
        [SerializeField, Range(0f, 1f)] private float airControl = 0.3f;

        private CharacterController controller;
        private MovementStatsAdapter stats;

        private Vector3 velocity;
        private Vector2 moveInput;

        private bool isSprinting;
        private bool isWalking;

        public bool IsCrouching { get; private set; }
        public Vector3 Velocity => velocity;
        public bool IsGrounded => controller.isGrounded;

        // ======================================================

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            stats = GetComponent<MovementStatsAdapter>();
            anim = GetComponent<PlayerAnimationController>();
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            // ❗ CharacterController работает ТОЛЬКО на сервере
            if (!IsServerInitialized)
                controller.enabled = false;
        }

        // ======================================================
        // INPUT API (вызывается сервером)
        // ======================================================

        public void SetMoveInput(Vector2 input) => moveInput = input;
        public void SetSprint(bool v) => isSprinting = v;
        public void SetWalk(bool v) => isWalking = v;

        public void TryJump()
        {
            if (!controller.isGrounded)
                return;

            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            anim?.TriggerJump();
        }

        public void ToggleCrouch()
        {
            IsCrouching = !IsCrouching;
        }

        public void SetBodyYaw(float yaw)
        {
            Vector3 euler = transform.eulerAngles;
            euler.y = yaw;
            transform.eulerAngles = euler;
        }

        // ======================================================
        // SERVER UPDATE LOOP
        // ======================================================

        private void Update()
        {
            if (!IsServerInitialized)
                return;

            HandleMovement();
            HandleCrouch();
            HandleAnimation();
        }

        // ======================================================

        private void HandleMovement()
        {
            float speed = GetSpeed();

            if (controller.isGrounded && velocity.y < 0)
                velocity.y = -2f;

            Vector3 dir =
                movementReference.forward * moveInput.y +
                movementReference.right * moveInput.x;

            if (controller.isGrounded)
            {
                velocity.x = dir.x * speed;
                velocity.z = dir.z * speed;
            }
            else
            {
                velocity += dir * speed * airControl * Time.deltaTime;
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void HandleCrouch()
        {
            float target = IsCrouching ? crouchHeight : standHeight;

            controller.height = Mathf.Lerp(
                controller.height,
                target,
                crouchTransitionSpeed * Time.deltaTime
            );

            controller.center = new Vector3(0, controller.height / 2f, 0);
        }

        private void HandleAnimation()
        {
            if (anim == null)
                return;

            float planar = new Vector2(velocity.x, velocity.z).magnitude;

            anim.SetSpeed(planar);
            anim.SetGrounded(controller.isGrounded);
            anim.SetCrouch(IsCrouching);
        }

        private float GetSpeed()
        {
            if (stats != null && stats.IsReady)
            {
                if (IsCrouching) return stats.CrouchSpeed;
                if (isSprinting) return stats.SprintSpeed;
                if (isWalking) return stats.WalkSpeed;
                return stats.BaseSpeed;
            }

            if (IsCrouching) return fallbackCrouchSpeed;
            if (isSprinting) return fallbackSprintSpeed;
            if (isWalking) return fallbackWalkSpeed;
            return fallbackBaseSpeed;
        }
    }
}
