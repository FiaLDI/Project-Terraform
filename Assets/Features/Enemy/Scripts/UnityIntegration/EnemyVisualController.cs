using UnityEngine;
using Unity.Entities;
using FishNet.Object;

public sealed class EnemyVisualController : NetworkBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private Entity entity;
    private EntityManager em;

    private EnemyAttackHandler attackHandler;
    private Vector3 lastPos;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        attackHandler = GetComponent<EnemyAttackHandler>();
        em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void Update()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null) return;
        }

        if (entity == Entity.Null || !em.Exists(entity))
        {
            var binder = GetComponent<EnemyEcsRuntimeBinder>();

            if (binder != null && binder.Entity != Entity.Null)
                entity = binder.Entity;

            return;
        }

        var attackState = em.GetComponentData<EnemyAttackState>(entity);

        // speed
        float speed = (transform.position - lastPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        animator.SetFloat(SpeedHash, speed);

        // attack
        if (attackState.DoAttack)
        {
            attackState.DoAttack = false;
            em.SetComponentData(entity, attackState);

            PlayAttackRpc();

            if (IsServer)
                attackHandler?.TriggerAttack();
        }

        lastPos = transform.position;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            transform.rotation,
            Time.deltaTime * 10f
        );
    }

    [ObserversRpc]
    private void PlayAttackRpc()
    {
        if (animator != null)
            animator.SetTrigger(AttackHash);
    }
}
