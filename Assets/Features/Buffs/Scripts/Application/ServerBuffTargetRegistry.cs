using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using Features.Buffs.Domain;

namespace Features.Buffs.Application
{
    /// <summary>
    /// Глобальный серверный реестр всех BuffTarget.
    /// НЕ зависит от физики, сцен, слоёв.
    /// </summary>
    public static class ServerBuffTargetRegistry
    {
        private static readonly HashSet<IBuffTarget> targets = new();

        public static IReadOnlyCollection<IBuffTarget> All => targets;

        public static void Register(IBuffTarget target)
        {
            if (target != null)
                targets.Add(target);
        }

        public static void Unregister(IBuffTarget target)
        {
            if (target != null)
                targets.Remove(target);
        }
    }
}
