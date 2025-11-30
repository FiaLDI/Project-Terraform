using UnityEngine;

namespace Features.Abilities.Domain
{
    public struct AbilityContext
    {
        /// <summary>Кто кастует способность (игрок / турель / дрон).</summary>
        public GameObject Owner;

        /// <summary>Камера прицеливания.</summary>
        public Camera AimCamera;

        /// <summary>Точка в мире, куда целимся (для point-абилок).</summary>
        public Vector3 TargetPoint;

        /// <summary>Направление способности (направление луча / эффекта).</summary>
        public Vector3 Direction;

        /// <summary>Индекс слота (0..N-1).</summary>
        public int SlotIndex;

        public AbilityContext(
            GameObject owner,
            Camera aimCamera,
            Vector3 targetPoint,
            Vector3 direction,
            int slotIndex
        )
        {
            Owner = owner;
            AimCamera = aimCamera;
            TargetPoint = targetPoint;
            Direction = direction;
            SlotIndex = slotIndex;
        }
    }
}
