using UnityEngine;
using Unity.Entities;
using FishNet.Object;
using Features.Enemy.Presentation.LOD;

public sealed class EnemyVisualController : NetworkBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    public Animator animator;

    private Entity entity;
    private EntityManager em;

    private EnemyAttackHandler attackHandler;
    private EnemyLODView lodView;

    private Vector3 lastPos;

    private void Awake()
    {
        attackHandler = GetComponent<EnemyAttackHandler>();
        lodView = GetComponent<EnemyLODView>();

        em = Unity.Entities.World.DefaultGameObjectInjectionWorld.EntityManager;

        if (lodView != null)
            lodView.OnModelChanged += OnModelChanged;
    }

    private void OnDestroy()
    {
        if (lodView != null)
            lodView.OnModelChanged -= OnModelChanged;
    }

    // =========================================================
    private void OnModelChanged(GameObject model)
    {
        if (model == null)
            return;

        animator = model.GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;
    }

    // =========================================================
    private void Update()
    {
        if (animator == null)
            return;

        if (entity == Entity.Null || !em.Exists(entity))
        {
            var binder = GetComponent<EnemyEcsRuntimeBinder>();

            if (binder != null && binder.Entity != Entity.Null && em.Exists(binder.Entity))
                entity = binder.Entity;

            return;
        }

        if (!em.HasComponent<EnemyAttackState>(entity))
            return;

        var attackState = em.GetComponentData<EnemyAttackState>(entity);

        // ===== SPEED =====
        float speed = (transform.position - lastPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        animator.SetFloat(SpeedHash, speed);

        // ===== ATTACK =====
        if (attackState.DoAttack)
        {
            attackState.DoAttack = false;
            em.SetComponentData(entity, attackState);

            PlayAttackRpc();

            if (IsServer)
                attackHandler?.TriggerAttack();
        }

        lastPos = transform.position;
    }

    // =========================================================
    [ObserversRpc]
    private void PlayAttackRpc()
    {
        if (animator != null)
            animator.SetTrigger(AttackHash);
    }
}
