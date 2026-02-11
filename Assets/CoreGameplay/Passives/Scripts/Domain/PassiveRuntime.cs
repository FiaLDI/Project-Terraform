
using Features.Buffs.Application;
using Features.Buffs.Domain;
using UnityEngine;

namespace Features.Passives.Domain
{
    /// <summary>
    /// Runtime-контейнер одного эффекта пассивки
    /// </summary>
    public sealed class PassiveRuntime : IBuffSource
    {
        public BuffInstance Buff;
        public Component Component;
    }
}