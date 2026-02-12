using UnityEngine;
using System;
using Features.Enemy.Data;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;
using Features.Buffs.Domain;
using Features.Buffs.Application;

namespace Features.Enemy.UnityIntegration
{
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(BuffSystem))]
    public sealed class EnemyActor :
        MonoBehaviour,
        IBuffTarget
    {
        private EnemyStats enemyStats;
        private BuffSystem buffSystem;

        public event Action OnReady;

        // =====================================================
        // UNITY
        // =====================================================

        private void Awake()
        {
            enemyStats = GetComponent<EnemyStats>();
            buffSystem = GetComponent<BuffSystem>();
        }

        private void Start()
        {
            if (enemyStats != null && enemyStats.IsReady)
            {
                OnReady?.Invoke();
            }
            else
            {
                Debug.LogError("[EnemyActor] Stats not ready", this);
            }
        }

        public IStatsFacade GetServerStats()
        {
            throw new NotImplementedException();
        }

        // =====================================================
        // IBuffTarget
        // =====================================================

        public BuffSystem BuffSystem => buffSystem;

        public GameObject GameObject => gameObject;

        public Transform Transform => transform;

        public bool IsReady => enemyStats != null && enemyStats.IsReady;

        public IBuffSource OwnerSource => null; // у врага нет владельца
    }
}
