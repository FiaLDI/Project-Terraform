using UnityEngine;
using Features.Buffs.Application;

namespace Features.Buffs.Domain
{
    public interface IBuffTarget
    {
        Transform Transform { get; }
        GameObject GameObject { get; }

        BuffSystem BuffSystem { get; }
    }
}
