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

        public bool AllowMovement { get; set; }

        private MovementStatsAdapter stats;

        public Vector3 Velocity => velocity;
        public bool IsGrounded => controller != null && controller.enabled ? controller.isGrounded : true;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            
            // Пытаемся найти stats
            stats = GetComponent<MovementStatsAdapter>();
            
            // Пытаемся найти anim
            anim = GetComponent<PlayerAnimationController>();
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            Debug.Log(
                $"[PlayerMovement] OnStartNetwork - " +
                $"IsOwner={base.Owner.IsLocalClient}, " +
                $"IsServer={IsServerInitialized}, " +
                $"GameObject={gameObject.name}",
                this
            );

            // ЛОГИКА 1: Отключаем CharacterController для чужих (proxy) объектов
            bool isRemoteProxy = !base.Owner.IsLocalClient && !IsServerInitialized;
            if (isRemoteProxy)
            {
                controller.enabled = false;
                Debug.Log($"[PlayerMovement] CharacterController disabled (remote proxy)", this);
            }

            // ЛОГИКА 2: Client Authoritative - разрешаем движение для Owner
            if (base.Owner.IsLocalClient || IsServerInitialized)
            {
                AllowMovement = true;
                Debug.Log($"[PlayerMovement] ✅ AllowMovement = TRUE (Client Authoritative)", this);
            }
        }

        // ================= INPUT API =================

        public void SetMoveInput(Vector2 input) => moveInput = input;
        public void SetSprint(bool sprinting) => isSprinting = sprinting;
        public void SetWalk(bool walking) => isWalking = walking;

        public void TryJump()
        {
            bool grounded = controller.enabled ? controller.isGrounded : false;

            if (grounded && !IsCrouching)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                if (anim != null) anim.TriggerJump();
                return;
            }

            if (grounded && IsCrouching)
            {
                ToggleCrouch();
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
                if (anim != null) anim.TriggerJump();
            }
        }

        public void ToggleCrouch()
        {
            if (IsCrouching)
            {
                if (CanStandUp())
                {
                    IsCrouching = false;
                    if (controller.enabled)
                    {
                        controller.height = standHeight;
                        controller.center = new Vector3(0, standHeight / 2f, 0);
                    }
                }
            }
            else
            {
                IsCrouching = true;
            }
        }

        // ================= UPDATE LOOP =================

        private void Update()
        {
            if (!AllowMovement) return;

            // Для чужих игроков (Proxy) не вычисляем физику
            if (!IsOwner)
            {
                HandleCrouchSmooth();
                return;
            }

            // Для владельца (Owner) вычисляем всё
            HandleAnimation();
            HandleMovement();
            HandleCrouchSmooth();
        }

        // ================= MOVEMENT LOGIC =================

        private void HandleMovement()
        {
            float currentSpeed = GetSpeedFromStats();

            bool grounded = controller.isGrounded;
            if (grounded && velocity.y < 0)
                velocity.y = -2f;

            Vector3 moveDirection = Vector3.zero;

            if (movementReference != null)
            {
                moveDirection = movementReference.forward * moveInput.y +
                                movementReference.right * moveInput.x;
            }

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

            if (controller.enabled)
            {
                controller.Move(velocity * Time.deltaTime);
            }
        }

        private void HandleCrouchSmooth()
        {
            if (!controller.enabled) return;

            float targetHeight = IsCrouching ? crouchHeight : standHeight;
            if (Mathf.Abs(controller.height - targetHeight) > 0.01f)
            {
                controller.height = Mathf.Lerp(
                    controller.height,
                    targetHeight,
                    crouchTransitionSpeed * Time.deltaTime
                );
                controller.center = new Vector3(0, controller.height / 2f, 0);
            }
        }

        // ================= ANIMATION =================

        private void HandleAnimation()
        {
            if (anim == null) return;

            float planarSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            bool grounded = controller.isGrounded;

            float animSpeed = 0f;

            if (grounded && planarSpeed > 0.1f)
            {
                if (IsCrouching)
                    animSpeed = 2f;
                else if (isSprinting)
                    animSpeed = 2f;
                else
                    animSpeed = 1f;
            }

            anim.SetSpeed(animSpeed);
            anim.SetGrounded(grounded);
            anim.SetCrouch(IsCrouching);
        }

        // ================= HELPERS =================

        private float GetSpeedFromStats()
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

        private bool CanStandUp()
        {
            RaycastHit hit;
            float checkDistance = standHeight - crouchHeight;
            Vector3 start = transform.position + Vector3.up * crouchHeight;
            float radius = controller != null ? controller.radius : 0.3f;

            return !Physics.SphereCast(start, radius, Vector3.up, out hit, checkDistance);
        }

        public void SetBodyYaw(float yaw)
        {
            Vector3 euler = transform.eulerAngles;
            euler.y = yaw;
            transform.eulerAngles = euler;
        }
    }
}