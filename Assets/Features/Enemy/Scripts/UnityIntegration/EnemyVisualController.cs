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

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private Vector3 lastPosition;

    private Entity entity;
    private EntityManager em;

    private EnemyAttackHandler attackHandler;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animatorController != null && animator != null)
            animator.runtimeAnimatorController = animatorController;

        if (modelRoot == null && animator != null)
            modelRoot = animator.transform;

        attackHandler = GetComponent<EnemyAttackHandler>();

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


        // ================= ATTACK =================
        var attackState = em.GetComponentData<EnemyAttackState>(entity);

        if (attackState.DoAttack)
        {
            animator.SetFloat(SpeedHash, 0.1f); // гарантируем выход из idle
            animator.SetTrigger(AttackHash);

            attackHandler?.TriggerAttack();

            attackState.DoAttack = false;
            em.SetComponentData(entity, attackState);
        }

        lastPosition = currentPosition;
    }
}
