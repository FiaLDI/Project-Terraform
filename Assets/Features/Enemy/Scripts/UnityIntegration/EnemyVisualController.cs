using UnityEngine;

public sealed class EnemyVisualController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [SerializeField] private RuntimeAnimatorController animatorController;

    [Header("Speed Settings")]
    [SerializeField] private float runSpeed = 3f;
    [SerializeField] private float dampTime = 0.1f;

    [Header("Rotation (Visual Only)")]
    [SerializeField] private Transform modelRoot;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float minTurnDistance = 0.01f;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private Vector3 lastPosition;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animatorController != null && animator != null)
            animator.runtimeAnimatorController = animatorController;

        if (modelRoot == null && animator != null)
            modelRoot = animator.transform;
    }

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (animator == null)
            return;

        Vector3 currentPosition = transform.position;

        Vector3 delta = currentPosition - lastPosition;
        delta.y = 0f;

        float rawSpeed =
            delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        float animSpeed = Mathf.Clamp01(rawSpeed / runSpeed);

        animator.SetFloat(
            SpeedHash,
            animSpeed,
            dampTime,
            Time.deltaTime
        );

        // Rotation (visual only)
        if (modelRoot != null &&
            delta.sqrMagnitude > minTurnDistance * minTurnDistance)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(delta.normalized);

            modelRoot.rotation = Quaternion.Slerp(
                modelRoot.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        lastPosition = currentPosition;
    }
}