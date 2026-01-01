using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using Features.Buffs.Domain;
using UnityEngine;

namespace Features.Buffs.Application
{
    /// <summary>
    /// Глобальный серверный реестр всех BuffTarget.
    /// Стабилен при late-join / host / despawn.
    /// </summary>
    public static class ServerBuffTargetRegistry
    {
        private static readonly HashSet<IBuffTarget> targets = new();
        private static readonly Dictionary<int, IBuffTarget> byNetId = new();

        public static IReadOnlyCollection<IBuffTarget> All => targets;

        // =====================================================
        // REGISTER
        // =====================================================

        public static void Register(IBuffTarget target)
        {
            if (!InstanceFinder.IsServer || target == null)
                return;

            if (target is not NetworkBehaviour nb)
                return;

            if (!nb.IsSpawned)
            {
                // 🔒 отложенная регистрация
                nb.StartCoroutine(WaitAndRegister(nb, target));
                return;
            }

            RegisterInternal(nb, target);
        }

        private static System.Collections.IEnumerator WaitAndRegister(
            NetworkBehaviour nb,
            IBuffTarget target)
        {
            yield return new WaitUntil(() => nb.IsSpawned);

            RegisterInternal(nb, target);
        }

        private static void RegisterInternal(NetworkBehaviour nb, IBuffTarget target)
        {
            var no = nb.NetworkObject;
            if (no == null)
                return;

            targets.Add(target);
            byNetId[no.ObjectId] = target;
        }


        // =====================================================
        // UNREGISTER
        // =====================================================

        public static void Unregister(IBuffTarget target)
        {
            if (!InstanceFinder.IsServer || target == null)
                return;

            if (target is not Component comp)
                return;

            if (!comp.TryGetComponent<NetworkObject>(out var no))
                return;

            targets.Remove(target);
            byNetId.Remove(no.ObjectId);
        }

        // =====================================================
        // LOOKUP
        // =====================================================

        public static IBuffTarget FindByNetId(int id)
        {
            return byNetId.TryGetValue(id, out var target)
                ? target
                : null;
        }

        // =====================================================
        // SAFETY
        // =====================================================

        public static void Clear()
        {
            targets.Clear();
            byNetId.Clear();
        }
    }
}
