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
        }
    }
}
