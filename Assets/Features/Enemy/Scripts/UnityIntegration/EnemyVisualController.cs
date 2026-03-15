using UnityEngine;
using Unity.Entities;

public sealed class EnemyVisualController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private RuntimeAnimatorController animatorController;

    [Header("Speed Settings")]
    [SerializeField] private float runSpeed = 3f;
    [SerializeField] private float dampTime = 0.1f;

    [Header("Rotation")]
    [SerializeField] private Transform modelRoot;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float minTurnDistance = 0.01f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private Vector3 lastPosition;

    private Entity entity;
    private EntityManager em;

    private EnemyAIState lastState;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animatorController != null && animator != null)
            animator.runtimeAnimatorController = animatorController;

        if (modelRoot == null && animator != null)
            modelRoot = animator.transform;

        em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void Start()
    {
        lastPosition = transform.position;

        var binder = GetComponent<EnemyEcsRuntimeBinder>();
        if (binder != null)
            entity = binder.Entity;
    }

    private void LateUpdate()
    {
        if (animator == null || !em.Exists(entity))
            return;

        // ================= SPEED =================
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

        // ================= ROTATION =================
        var targetData = em.GetComponentData<EnemyTargetPosition>(entity);

        Vector3 targetPos = targetData.Value;

        Vector3 lookDir = targetPos - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);

            modelRoot.rotation = Quaternion.Slerp(
                modelRoot.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
        
        lastPosition = currentPosition;
    }
}