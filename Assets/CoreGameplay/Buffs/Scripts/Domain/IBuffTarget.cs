using UnityEngine;
using Features.Buffs.Application;
using Features.Stats.Domain;
using System;

namespace Features.Buffs.Domain
{
    public interface IBuffTarget
    {
        Transform Transform { get; }
        GameObject GameObject { get; }

        BuffSystem BuffSystem { get; }

        IStatsFacade GetServerStats();

        bool IsReady { get; }
        event Action OnReady;
    }
}
