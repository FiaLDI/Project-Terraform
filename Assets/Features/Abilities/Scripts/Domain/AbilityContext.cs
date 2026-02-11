using UnityEngine;
using Features.Buffs.Domain;

namespace Features.Abilities.Domain
{
    public struct AbilityContext
    {
        public IBuffSource Owner;

        public Vector3 TargetPoint;
        public Vector3 Direction;

        public int SlotIndex;
        public float Yaw;
        public float Pitch;

        public AbilityContext(
            IBuffSource owner,
            Vector3 targetPoint,
            Vector3 direction,
            int slotIndex,
            float yaw,
            float pitch)
        {
            Owner = owner;
            TargetPoint = targetPoint;
            Direction = direction;
            SlotIndex = slotIndex;
            Yaw = yaw;
            Pitch = pitch;
        }
    }
}
