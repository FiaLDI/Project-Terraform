using UnityEngine;

public sealed class EnemyVisualController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Tooltip("Animator Controller для врага")]
    [SerializeField] private RuntimeAnimatorController animatorController;

    [Header("Movement Source")]
    [SerializeField] private Rigidbody rb;

    [Header("Speed Settings")]
    [SerializeField] private float walkSpeed = 0.2f;
    [SerializeField] private float runSpeed = 2.5f;
    [SerializeField] private float dampTime = 0.1f;

    [Header("Rotation (Visual Only)")]
    [SerializeField] private Transform modelRoot;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float minTurnDistance = 0.2f;

    private Vector3 lastPosition;
    private EnemyEcsMoveBridge moveBridge;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animatorController != null)
            animator.runtimeAnimatorController = animatorController;

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (modelRoot == null && animator != null)
            modelRoot = animator.transform;

        moveBridge = GetComponent<EnemyEcsMoveBridge>();

        lastPosition = transform.position;
    }

    private void Update()
    {
        // =========================
        // SPEED → ANIMATION
        // =========================
        Vector3 velocity =
            (transform.position - lastPosition) / Time.deltaTime;

        Vector3 horizontalVelocity = new Vector3(
            velocity.x,
            0f,
            velocity.z
        );

        float speed = horizontalVelocity.magnitude;

        float animSpeed = 0f;
        if (speed > walkSpeed)
        {
            animSpeed = Mathf.InverseLerp(
                walkSpeed,
                runSpeed,
                speed
            );
        }

        animator.SetFloat(
            SpeedHash,
            animSpeed,
            dampTime,
            Time.deltaTime
        );

        // =========================
        // ROTATION → BY AI TARGET
        // =========================
        if (moveBridge != null)
        {
            Vector3 toTarget =
                moveBridge.CurrentTarget - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude >
                minTurnDistance * minTurnDistance)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(toTarget.normalized);

                modelRoot.rotation = Quaternion.Slerp(
                    modelRoot.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        lastPosition = transform.position;
    }
}
