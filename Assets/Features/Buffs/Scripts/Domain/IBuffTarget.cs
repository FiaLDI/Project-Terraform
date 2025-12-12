using UnityEngine;
using Features.Buffs.Application;
using Features.Stats.Domain;

namespace Features.Buffs.Domain
{
    public interface IBuffTarget
    {
        Transform Transform { get; }
        GameObject GameObject { get; }

        BuffSystem BuffSystem { get; }

        IStatsFacade Stats { get; }
    }
}
