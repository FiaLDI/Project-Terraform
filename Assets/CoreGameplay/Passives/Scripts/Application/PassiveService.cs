using System.Collections.Generic;
using Features.Passives.Domain;
using Features.Passives.UnityIntegration;
using Features.Buffs.Domain;
using Features.Stats.UnityIntegration;

namespace Features.Passives.Application
{
    /// <summary>
    /// Истина о пассивках у конкретной сущности.
    /// Не знает Unity.
    /// Не знает Network.
    /// </summary>
    public sealed class PassiveService
    {
        private readonly StatsBuffTarget target;

        // Активные пассивки
        private readonly List<PassiveSO> active = new();

        // Runtime-источники (1 passive = 1 source)
        private readonly Dictionary<PassiveSO, PassiveSource> sources = new();
        
        private readonly List<AbilityModifierSO> cachedModifiers = new();
        public IReadOnlyList<AbilityModifierSO> CachedModifiers => cachedModifiers;

        public IReadOnlyList<PassiveSO> Active => active;

        public PassiveService(StatsBuffTarget target)
        {
            this.target = target;
        }

        // =====================================================
        // SET
        // =====================================================

        public void Set(IEnumerable<PassiveSO> passives)
        {
            ClearAll();

            if (passives == null)
                return;

            foreach (var p in passives)
                Activate(p);
            
            RebuildModifierCache();
        }

        // =====================================================
        // ACTIVATE
        // =====================================================

        private void Activate(PassiveSO so)
        {
            if (so == null || active.Contains(so))
                return;

            active.Add(so);

            var source = new PassiveSource(so);
            sources[so] = source;

            foreach (var effect in so.effects)
            {
                var data = effect.Build();
                PassiveExecutor.Instance.Apply(data, target, source);
            }

            if (so.abilityModifiers != null)
            {
                for (int i = 0; i < so.abilityModifiers.Count; i++)
                {
                    cachedModifiers.Add(so.abilityModifiers[i]);
                }
            }
        }

        // =====================================================
        // DEACTIVATE
        // =====================================================

        public void Deactivate(PassiveSO so)
        {
            if (so == null || !sources.TryGetValue(so, out var source))
                return;

            target.BuffSystem.RemoveBySource(source);

            sources.Remove(so);
            active.Remove(so);
        }

        // =====================================================
        // CLEAR
        // =====================================================

        public void ClearAll()
        {
            foreach (var pair in sources)
                target.BuffSystem.RemoveBySource(pair.Value);

            sources.Clear();
            active.Clear();
            cachedModifiers.Clear();
        }

        private void RebuildModifierCache()
        {
            cachedModifiers.Clear();

            for (int i = 0; i < active.Count; i++)
            {
                var p = active[i];

                if (p.abilityModifiers == null)
                    continue;

                for (int j = 0; j < p.abilityModifiers.Count; j++)
                {
                    cachedModifiers.Add(p.abilityModifiers[j]);
                }
            }
        }
    }
}
