using UnityEngine;
using System.Collections;
using Features.Effects.Domain;
using Features.Effects.Application;
using Features.Buffs.Domain;

public sealed class EnemyAttackHandler : MonoBehaviour
{
    [Header("Attack Timing")]
    [SerializeField] private float attackDelay = 0.3f;

    [Header("Effect")]
    [SerializeField] private EffectDefinition effect;

    private bool isAttacking;

    public void TriggerAttack()
    {
        if (!isAttacking)
            StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        yield return new WaitForSeconds(attackDelay);

        DealDamage();

        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
    }

    private void DealDamage()
    {
        Vector3 forward = transform.forward;

        var visual = GetComponent<EnemyVisualController>();
        if (visual != null)
        {
            var root = visual.GetComponentInChildren<Animator>()?.transform;
            if (root != null)
                forward = root.forward;
        }

        Vector3 origin = transform.position + forward * 1.2f;

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
            forward
        );

        var targets = TargetResolver.Resolve(effect, ctx);

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