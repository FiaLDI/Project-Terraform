using UnityEngine;
using Features.Stats.Adapter;   // <-- важно

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform movementReference;
    [SerializeField] private Transform cameraReference;
    [SerializeField] private Animator animator;

    [Header("Rotation alignment")]
    [SerializeField] private Transform headTransform;
    [SerializeField] private float alignSpeed = 120f;
    [SerializeField] private float alignThreshold = 1f;
    [SerializeField] private float alignStartAngle = 5f;
    [SerializeField] private Transform cameraPivot;

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
    [SerializeField][Range(0f, 1f)] private float airControl = 0.3f;

    public bool IsCrouching { get; private set; } = false;

    public Vector2 moveInput = Vector2.zero;
    private Vector3 velocity = Vector3.zero;
    private CharacterController controller;

    private bool isSprinting = false;
    private bool isWalking = false;
    private bool isAligning = false;

    // 🔥 NEW: Movement stats adapter
    private MovementStatsAdapter stats;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        stats = GetComponent<MovementStatsAdapter>(); // получаем адаптер, если он есть
    }

    // =====================================================================
    // Input API
    // =====================================================================

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public void SetSprint(bool sprinting)
    {
        isSprinting = sprinting;
    }

    public void SetWalk(bool walking)
    {
        isWalking = walking;
    }

    public void TryJump()
    {
        if (controller.isGrounded && !IsCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            animator.SetTrigger("jump");
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

    // =====================================================================
    // Update loop
    // =====================================================================

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

        // Fallback (editor / testing)
        if (IsCrouching) return fallbackCrouchSpeed;
        if (isSprinting) return fallbackSprintSpeed;
        if (isWalking) return fallbackWalkSpeed;
        return fallbackBaseSpeed;
    }

    private void HandleMovement()
    {
        float currentSpeed = GetSpeedFromStats();

        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector3 moveDirection =
            movementReference.forward * moveInput.y +
            movementReference.right * moveInput.x;

        // Ground or air movement
        if (isGrounded)
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

        HandleBodyAlignment(moveDirection);
    }

    private void HandleBodyAlignment(Vector3 moveDirection)
    {
        if (moveInput.y > 0.1f && cameraReference != null && movementReference != null)
        {
            Vector3 bodyForward = movementReference.forward;
            Vector3 camForward = cameraReference.forward;

            bodyForward.y = 0;
            camForward.y = 0;

            float signedAngle = Vector3.SignedAngle(bodyForward, camForward, Vector3.up);

            if (Mathf.Abs(signedAngle) > alignStartAngle)
                isAligning = true;

            if (isAligning)
            {
                float turnDir = Mathf.Sign(signedAngle);
                float turnAmount = alignSpeed * Time.deltaTime * turnDir;

                if (Mathf.Abs(signedAngle) <= alignThreshold)
                {
                    isAligning = false;
                }
                else
                {
                    if (Mathf.Abs(turnAmount) > Mathf.Abs(signedAngle))
                        turnAmount = signedAngle;

                    movementReference.Rotate(Vector3.up * turnAmount, Space.World);

                    if (headTransform != null)
                    {
                        cameraPivot.Rotate(Vector3.up * -turnAmount, Space.World);
                    }
                }
            }
        }
        else
        {
            isAligning = false;
        }
    }

    private bool CanStandUp()
    {
        RaycastHit hit;
        float checkDistance = standHeight - crouchHeight;
        Vector3 start = transform.position + Vector3.up * crouchHeight;

        return !Physics.SphereCast(start, controller.radius, Vector3.up, out hit, checkDistance);
    }

    // =====================================================================
    // Animation
    // =====================================================================

    private void HandleAnimation()
    {
        float planarSpeed = new Vector2(velocity.x, velocity.z).magnitude;
        bool isMoving = planarSpeed > 0.1f && controller.isGrounded;

        animator.SetBool("walk", isMoving);
        animator.SetBool("run", isMoving && isSprinting);
        animator.SetBool("crouch", IsCrouching);
        animator.SetBool("sit_walk", IsCrouching && isMoving);

        bool movingForwardBackward = Mathf.Abs(moveInput.y) > 0.1f;
        bool turning = !movingForwardBackward && Mathf.Abs(moveInput.x) > 0.1f;

        animator.SetBool("turn", turning);
    }
}
