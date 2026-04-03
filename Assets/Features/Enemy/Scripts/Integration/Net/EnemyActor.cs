using UnityEngine;
using System;
using Features.Enemy.Data;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using FishNet.Object;

namespace Features.Enemy.UnityIntegration
{
    [RequireComponent(typeof(EnemyStats))]
    [RequireComponent(typeof(BuffSystem))]
    public sealed class EnemyActor :
        NetworkBehaviour,
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
            if (IsServer)
            {
                if (enemyStats != null && enemyStats.IsReady)
                    OnReady?.Invoke();
            }
            else
            {
                // клиент сразу готов (ждёт снапшот)
                OnReady?.Invoke();
            }
        }

        public IStatsFacade GetServerStats()
        {
            if (enemyStats == null || !enemyStats.IsReady)
                return null;

            return enemyStats.Facade;
        }

        // =====================================================
        // IBuffTarget
        // =====================================================

        public BuffSystem BuffSystem => buffSystem;

        public GameObject GameObject => gameObject;

        public Transform Transform => transform;

        public bool IsReady =>
            IsServer
                ? (enemyStats != null && enemyStats.IsReady)
                : true;

        public IBuffSource OwnerSource => null; // у врага нет владельца
    }
}
