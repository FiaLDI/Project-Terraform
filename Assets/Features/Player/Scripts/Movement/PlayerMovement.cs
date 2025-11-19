using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovementStats stats;
    [SerializeField] private Transform movementReference;
    [SerializeField] private Transform cameraReference;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform headTransform;
    [SerializeField] private Transform cameraPivot;

    [Header("Rotation alignment")]
    [SerializeField] private float alignSpeed = 120f;
    [SerializeField] private float alignThreshold = 1f;
    [SerializeField] private float alignStartAngle = 5f;

    public bool IsCrouching { get; private set; } = false;

    public Vector2 moveInput = Vector2.zero;
    private Vector3 velocity = Vector3.zero;
    private CharacterController controller;

    private bool isSprinting = false;
    private bool isWalking = false;
    private bool isAligning = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // ============= INPUT FROM PlayerController =============

    public void SetMoveInput(Vector2 input) => moveInput = input;
    public void SetSprint(bool sprinting)   => isSprinting = sprinting;
    public void SetWalk(bool walking)       => isWalking = walking;

    public void TryJump()
    {
        if (controller.isGrounded && !IsCrouching)
        {
            velocity.y = Mathf.Sqrt(stats.jumpForce * -2f * stats.gravity);
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
                controller.height = stats.standHeight;
                controller.center = new Vector3(0, stats.standHeight * 0.5f, 0);
            }
        }
        else
        {
            IsCrouching = true;
            controller.height = stats.crouchHeight;
            controller.center = new Vector3(0, stats.crouchHeight * 0.5f, 0);
        }
    }

    // ================== MAIN UPDATE ====================

    private void Update()
    {
        HandleMovement();
        HandleAnimation();
    }

    private void HandleMovement()
    {
        float currentSpeed = stats.GetSpeed(IsCrouching, isSprinting, isWalking);

        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        Vector3 moveDir =
            movementReference.forward * moveInput.y +
            movementReference.right * moveInput.x;

        if (isGrounded)
        {
            velocity.x = moveDir.x * currentSpeed;
            velocity.z = moveDir.z * currentSpeed;
        }
        else
        {
            velocity += moveDir * currentSpeed * stats.airControl * Time.deltaTime;
        }

        velocity.y += stats.gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        HandleRotationAlignment();
    }

    private void HandleRotationAlignment()
    {
        if (moveInput.y <= 0.1f || cameraReference == null)
        {
            isAligning = false;
            return;
        }

        Vector3 bodyForward = movementReference.forward;
        Vector3 camForward = cameraReference.forward;
        bodyForward.y = camForward.y = 0;

        float signedAngle = Vector3.SignedAngle(bodyForward, camForward, Vector3.up);

        if (Mathf.Abs(signedAngle) > alignStartAngle)
            isAligning = true;

        if (!isAligning) return;

        float turnDir = Mathf.Sign(signedAngle);
        float turnAmount = alignSpeed * Time.deltaTime * turnDir;

        if (Mathf.Abs(signedAngle) <= alignThreshold)
            isAligning = false;

        if (Mathf.Abs(turnAmount) > Mathf.Abs(signedAngle))
            turnAmount = signedAngle;

        movementReference.Rotate(Vector3.up * turnAmount, Space.World);
        cameraPivot.Rotate(Vector3.up * -turnAmount, Space.World);
    }

    // ============= CROUCH STAND-UP CHECK =============

    private bool CanStandUp()
    {
        float checkDistance = stats.standHeight - stats.crouchHeight;
        Vector3 start = transform.position + Vector3.up * stats.crouchHeight;

        return !Physics.SphereCast(start, controller.radius, Vector3.up, out RaycastHit hit, checkDistance);
    }

    // ============= ANIMATION =============

    private void HandleAnimation()
    {
        float planarSpeed = new Vector2(velocity.x, velocity.z).magnitude;
        bool isMoving = planarSpeed > 0.1f && controller.isGrounded;

        animator.SetBool("walk", isMoving);
        animator.SetBool("run", isMoving && isSprinting);
        animator.SetBool("crouch", IsCrouching);
        animator.SetBool("sit_walk", IsCrouching && isMoving);

        bool movingFB = Mathf.Abs(moveInput.y) > 0.1f;
        bool turning = !movingFB && Mathf.Abs(moveInput.x) > 0.1f;

        animator.SetBool("turn", turning);
    }
}
