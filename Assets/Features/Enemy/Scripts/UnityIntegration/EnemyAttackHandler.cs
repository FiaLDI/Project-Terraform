using UnityEngine;
using System.Collections;
using Features.Effects.Domain;
using Features.Effects.Application;
using Features.Buffs.Domain;
using Features.Player.UnityIntegration;

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
        var registry = PlayerRegistry.Instance;
        if (registry == null || registry.LocalPlayer == null)
            return;

        Vector3 targetPos = registry.LocalPlayer.transform.position;

        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        dir.Normalize();

        Vector3 origin = transform.position;
        origin.y = targetPos.y;

        origin += dir * 1.2f;

        IBuffSource source = null;

        foreach (var m in GetComponentsInParent<MonoBehaviour>())
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
            dir
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