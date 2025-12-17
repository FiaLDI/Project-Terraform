// Assets/Features/Player/Scripts/UnityIntegration/PlayerMovement.cs
using UnityEngine;
using Features.Stats.Adapter;

namespace Features.Player.UnityIntegration
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform movementReference; 
        [SerializeField] private Transform cameraReference;
        private PlayerAnimationController anim;

        [Header("Editor Fallback Speeds (Used if Stats not yet initialized)")]
        [SerializeField] private float fallbackBaseSpeed = 5f;
        [SerializeField] private float fallbackWalkSpeed = 2f;
        [SerializeField] private float fallbackSprintSpeed = 8f;
        [SerializeField] private float fallbackCrouchSpeed = 1.5f;

        [Header("Jump & Gravity")]
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float jumpForce = 5f;

        [Header("Crouch Settings")]
        [SerializeField] private float standHeight = 2f;
        [SerializeField] private float crouchHeight = 1f;
        [SerializeField] private float crouchTransitionSpeed = 10f;

        [Header("Air Control")]
        [SerializeField, Range(0f, 1f)] private float airControl = 0.3f;

        public bool IsCrouching { get; private set; }

        public Vector2 moveInput = Vector2.zero;
        private Vector3 velocity = Vector3.zero;
        private CharacterController controller;

        private bool isSprinting;
        private bool isWalking;

        private MovementStatsAdapter stats;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            stats = GetComponent<MovementStatsAdapter>();
            anim = GetComponent<PlayerAnimationController>();
        }

        // Input API
        public void SetMoveInput(Vector2 input) => moveInput = input;
        public void SetSprint(bool sprinting) => isSprinting = sprinting;
        public void SetWalk(bool walking) => isWalking = walking;

        public void TryJump()
        {
            if (controller.isGrounded && !IsCrouching)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                anim?.TriggerJump();
                return;
            }

            if (controller.isGrounded && IsCrouching)
            {
                ToggleCrouch();
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                anim?.TriggerJump();
            }
        }

        public void ToggleCrouch()
        {
            if (IsCrouching)
            {
                if (CanStandUp())
                {
                    IsCrouching = false;
                    controller.height = standHeight;
                    controller.center = new Vector3(0, standHeight / 2f, 0);
                }
            }
            else
            {
                IsCrouching = true;
                controller.height = crouchHeight;
                controller.center = new Vector3(0, crouchHeight / 2f, 0);
            }
        }

        private void Update()
        {
            HandleMovement();
            HandleAnimation();
        }

        private float GetSpeedFromStats()
        {
            if (stats != null)
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

        private void HandleMovement()
        {
            float currentSpeed = GetSpeedFromStats();

            bool grounded = controller.isGrounded;
            if (grounded && velocity.y < 0)
                velocity.y = -2f;

            Vector3 moveDirection =
                movementReference.forward * moveInput.y +
                movementReference.right * moveInput.x;

            if (grounded)
            {
                velocity.x = moveDirection.x * currentSpeed;
                velocity.z = moveDirection.z * currentSpeed;
            }
            else
            {
                velocity += moveDirection * currentSpeed * airControl * Time.deltaTime;
            }

            velocity.y += gravity * Time.deltaTime;

            controller.Move(velocity * Time.deltaTime);
        }

        private bool CanStandUp()
        {
            RaycastHit hit;
            float checkDistance = standHeight - crouchHeight;
            Vector3 start = transform.position + Vector3.up * crouchHeight;

            return !Physics.SphereCast(start, controller.radius, Vector3.up, out hit, checkDistance);
        }

        // Animation
        private void HandleAnimation()
        {
            if (anim == null)
                return;

            float planarSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            bool grounded = controller.isGrounded;

            float animSpeed = 0f;

            if (grounded && planarSpeed > 0.1f)
            {
                if (IsCrouching)
                    animSpeed = 2f;        // Sit_Walk
                else if (isSprinting)
                    animSpeed = 2f;        // Running
                else
                    animSpeed = 1f;        // Walking
            }

            anim.SetSpeed(animSpeed);
            anim.SetGrounded(grounded);
            anim.SetCrouch(IsCrouching);
        }

    }
}
