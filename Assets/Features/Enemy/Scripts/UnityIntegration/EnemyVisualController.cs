using UnityEngine;
using Unity.Entities;
using FishNet.Object;

public sealed class EnemyVisualController : NetworkBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private RuntimeAnimatorController animatorController;

    [Header("Speed Settings")]
    [SerializeField] private float runSpeed = 3f;
    [SerializeField] private float dampTime = 0.1f;

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

        attackHandler = GetComponent<EnemyAttackHandler>();

        em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update() // ❗ НЕ LateUpdate
    {
        if (entity == Entity.Null || !em.Exists(entity))
        {
            var binder = GetComponent<EnemyEcsRuntimeBinder>();

            if (binder != null)
            {
                var newEntity = binder.Entity;

                if (newEntity != Entity.Null && em.Exists(newEntity))
                    entity = newEntity;
            }
        }

        if (animator == null || entity == Entity.Null || !em.Exists(entity))
            return;

        var attackState = em.GetComponentData<EnemyAttackState>(entity);

        // ================= SPEED =================
        Vector3 currentPosition = transform.position;

        Vector3 delta = currentPosition - lastPosition;
        delta.y = 0f;

        float rawSpeed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        float animSpeed = Mathf.Clamp01(rawSpeed / runSpeed);

        animator.SetFloat(SpeedHash, animSpeed, dampTime, Time.deltaTime);

        // ================= ATTACK =================
        if (attackState.DoAttack)
        {
            attackState.DoAttack = false;
            em.SetComponentData(entity, attackState);

            PlayAttackClientRpc(); // 👈 анимация всем

            if (IsServer)
                attackHandler?.TriggerAttack(); // 👈 урон только сервер
        }

        lastPosition = currentPosition;
    }

    [ObserversRpc]
    private void PlayAttackClientRpc()
    {
        if (animator != null)
            animator.SetTrigger(AttackHash);
    }
}
