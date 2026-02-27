using UnityEngine;

public sealed class EnemyVisualController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Tooltip("Animator Controller для врага")]
    [SerializeField] private RuntimeAnimatorController animatorController;

    [Header("Speed Settings")]
    [SerializeField] private float runSpeed = 3f;   // должно совпадать с moveSpeed
    [SerializeField] private float dampTime = 0.1f;

    [Header("Rotation (Visual Only)")]
    [SerializeField] private Transform modelRoot;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float minTurnDistance = 0.2f;

    private EnemyEcsMoveBridge moveBridge;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animatorController != null)
            animator.runtimeAnimatorController = animatorController;

        if (modelRoot == null && animator != null)
            modelRoot = animator.transform;

        moveBridge = GetComponent<EnemyEcsMoveBridge>();
    }

    private void Update()
    {
        if (animator == null)
            return;

        // =========================
        // SPEED → ANIMATION
        // =========================
        float speed = moveBridge != null
            ? moveBridge.CurrentSpeed
            : 0f;

        float animSpeed = Mathf.Clamp01(speed / runSpeed);

        animator.SetFloat(
            SpeedHash,
            animSpeed,
            dampTime,
            Time.deltaTime
        );

        // =========================
        // ROTATION → BY AI TARGET
        // =========================
        if (moveBridge != null && modelRoot != null)
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
    }
}