using UnityEngine;
using System.Collections;
using Unity.Entities;
using Features.Effects.Domain;
using Features.Effects.Application;
using Features.Buffs.Domain;

public sealed class EnemyAttackHandler : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Attack Timing")]
    [SerializeField] private float attackDelay = 0.3f;

    [Header("Effect")]
    [SerializeField] private EffectDefinition effect;

    private Entity entity;
    private EntityManager em;

    private bool isAttacking;

    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        em = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        var binder = GetComponent<EnemyEcsRuntimeBinder>();
        if (binder != null)
            entity = binder.Entity;
    }

    private void Update()
    {
        if (!em.Exists(entity))
            return;

        var attack = em.GetComponentData<EnemyAttackState>(entity);

        if (attack.DoAttack && !isAttacking)
        {
            // триггерим анимацию
            animator.SetTrigger(AttackHash);

            // запускаем атаку
            StartCoroutine(AttackRoutine());

            attack.DoAttack = false;
            em.SetComponentData(entity, attack);
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // ⏱ ждём момент удара
        yield return new WaitForSeconds(attackDelay);

        DealDamage();

        // можно добавить recovery
        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
    }

    private void DealDamage()
    {
        Debug.Log("ATTACK DAMAGE");

        Vector3 origin = transform.position + transform.forward * 1.2f;

        // 🔥 получаем source правильно
        IBuffSource source = null;

        var monos = GetComponentsInParent<MonoBehaviour>();
        foreach (var m in monos)
        {
            if (m is IBuffSource s)
            {
                source = s;
                break;
            }
        }

        var ctx = new EffectContext(
            source,
            null,
            origin,
            transform.forward
        );

        var targets = TargetResolver.Resolve(effect, ctx);

        Debug.Log("Targets: " + targets.Length);

        if (targets.Length == 0)
            return;

        var newCtx = new EffectContext(
            ctx.Source,
            targets,
            ctx.Origin,
            ctx.Direction
        );

        var effectInstance = EffectFactory.Create(effect);
        effectInstance?.Apply(newCtx);
    }
}
