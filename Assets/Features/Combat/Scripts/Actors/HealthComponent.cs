using Features.Buffs.Application;
using Features.Buffs.Domain;
using Features.Stats.Domain;
using UnityEngine;

namespace Features.Combat.Actors
{
    public sealed class HealthComponent : MonoBehaviour, IBuffTarget
    {
        [Header("Stats Owner")]
        [SerializeField] private MonoBehaviour statsOwnerBehaviour;

        private IStatsOwner statsOwner;
        private IStatsFacade stats;
        private BuffSystem buffSystem;

        public event System.Action OnReady;

        // ======================================================
        // IBuffTarget
        // ======================================================

        public BuffSystem BuffSystem => buffSystem;
        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public bool IsReady => statsOwner != null && statsOwner.IsReady;
        public IBuffSource OwnerSource => statsOwner as IBuffSource;

        public IStatsFacade GetServerStats() => stats;

        // ======================================================
        // UNITY
        // ======================================================

        private void Awake()
        {
            statsOwner = statsOwnerBehaviour as IStatsOwner
                ?? GetComponent<IStatsOwner>();

            buffSystem = GetComponent<BuffSystem>();

            if (statsOwner == null)
            {
                Debug.LogError("[HealthComponent] IStatsOwner missing", this);
                enabled = false;
                return;
            }
        }

        private void Start()
        {
            if (!statsOwner.IsReady)
                return;

            stats = statsOwner.Facade;
            OnReady?.Invoke();
        }

        // ======================================================
        // DIRECT API (optional)
        // ======================================================

        public void TakeDamage(float amount)
        {
            stats?.Health?.Damage(amount);
        }

        public void Heal(float amount)
        {
            stats?.Health?.Heal(amount);
        }
    }
}
