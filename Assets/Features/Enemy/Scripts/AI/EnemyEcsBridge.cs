using Features.Enemy.UnityIntegration;
using Unity.Entities;
using UnityEngine;

public sealed class EnemyEcsBridge : MonoBehaviour
{
    public Entity Entity;
    public EntityManager EntityManager;

    private EnemyActor actor;

    private void Awake()
    {
        actor = GetComponent<EnemyActor>();
        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void Update()
    {
        if (!EntityManager.Exists(Entity))
            return;

        var state = EntityManager.GetComponentData<EnemyState>(Entity);

        if (state.Value == EnemyAIState.Attack)
        {
            TryDealDamage();
        }
    }

    private void TryDealDamage()
    {
        // поиск игрока → IDamageable
        // actor.TakeDamage(...) НЕ НУЖНО
        // combat.ApplyDamage(player, ...)
    }
}
