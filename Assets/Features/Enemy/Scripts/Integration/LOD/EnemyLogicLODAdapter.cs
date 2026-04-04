using UnityEngine;
using Unity.Entities;
using Features.Enemy.UnityIntegration;

namespace Features.Enemy.Integration.LOD
{
    public class EnemyLogicLODAdapter : MonoBehaviour
    {
        private EnemyEcsMoveBridge bridge;
        private EnemyAttackHandler attack;

        private Entity entity;
        private EntityManager em;

        private void Awake()
        {
            bridge = GetComponent<EnemyEcsMoveBridge>();
            attack = GetComponent<EnemyAttackHandler>();

            em = World.DefaultGameObjectInjectionWorld.EntityManager;

            var binder = GetComponent<EnemyEcsRuntimeBinder>();
            if (binder != null)
                entity = binder.Entity;
        }

        public void ApplyLOD(int lod)
        {
            if (em.Exists(entity))
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
    }
}
