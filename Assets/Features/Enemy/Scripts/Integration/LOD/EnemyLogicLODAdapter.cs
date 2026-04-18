using UnityEngine;
using Unity.Entities;
using Features.Enemy.UnityIntegration;

namespace Features.Enemy.Integration.LOD
{
    public class EnemyLogicLODAdapter : MonoBehaviour
    {
        private EnemyEcsMoveBridge bridge;
        private EnemyAttackHandler attack;
        private EnemyEcsRuntimeBinder binder;

        private Entity entity;
        private EntityManager em;

        private void Awake()
        {
            bridge = GetComponent<EnemyEcsMoveBridge>();
            attack = GetComponent<EnemyAttackHandler>();
            binder = GetComponent<EnemyEcsRuntimeBinder>();
        }

        public void ApplyLOD(int lod)
        {
            TryResolveEntity();

            if (em != default && entity != Entity.Null && em.Exists(entity))
            {
                if (lod >= 2)
                {
                    if (!em.HasComponent<EnemyInactive>(entity))
                        em.AddComponent<EnemyInactive>(entity);
                }
                else
                {
                    if (em.HasComponent<EnemyInactive>(entity))
                        em.RemoveComponent<EnemyInactive>(entity);
                }
            }

            if (bridge != null)
                bridge.enabled = lod < 2;

            if (attack != null)
                attack.enabled = lod < 2;
        }

        private void TryResolveEntity()
        {
            if (em == default && World.DefaultGameObjectInjectionWorld != null)
                em = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (em == default || binder == null)
                return;

            if (entity != Entity.Null && em.Exists(entity))
                return;

            if (binder.Entity == Entity.Null || !em.Exists(binder.Entity))
                return;

            entity = binder.Entity;
        }
    }
}
