using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform movementReference;
    [SerializeField] private Animator animator;

    [Header("Movement Parameters")]
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 1.5f;
    
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

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

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
                controller.height = 3;
                controller.center = new Vector3(0, 1.5f, 0);
            }
        }
        else
        {
            IsCrouching = true;
            controller.height = 2.5f;
            controller.center = new Vector3(0, 1.25f, 0);
        }
    }


    private void Update()
    {
        HandleMovement();
        HandleAnimation();
    }

    private void HandleMovement()
    {
        float currentSpeed =
            IsCrouching ? crouchSpeed :
            isSprinting ? sprintSpeed :
            isWalking ? walkSpeed :
            baseSpeed;

        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector3 moveDirection = movementReference.forward * moveInput.y + movementReference.right * moveInput.x;

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
    }

    private bool CanStandUp()
    {
        RaycastHit hit;
        float checkDistance = standHeight - crouchHeight;
        Vector3 start = transform.position + Vector3.up * crouchHeight;
        return !Physics.SphereCast(start, controller.radius, Vector3.up, out hit, checkDistance);
    }



    private void HandleAnimation()
    {
        float planarSpeed = new Vector2(velocity.x, velocity.z).magnitude;
        bool isMoving = planarSpeed > 0.1f && controller.isGrounded;

        animator.SetBool("walk", isMoving);
        animator.SetBool("run", isMoving && isSprinting);
        //animator.SetBool("crouch", IsCrouching && !isMoving);
        animator.SetBool("crouch", IsCrouching );
        animator.SetBool("sit_walk", IsCrouching && isMoving);

        bool movingForwardBackward = Mathf.Abs(moveInput.y) > 0.1f;

 
        bool turning = !movingForwardBackward && Mathf.Abs(moveInput.x) > 0.1f;
        animator.SetBool("turn", turning);
    }
}
