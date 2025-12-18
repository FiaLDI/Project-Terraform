using UnityEngine;

namespace Features.Abilities.Domain
{
    /// <summary>
    /// Контекст выполнения способности в ЧИСТОМ виде.
    /// Не содержит Unity-компонентов.
    /// Содержит только данные, нужные сервисам.
    /// </summary>
    public struct AbilityContext
    {
        /// <summary>ID или ссылка на владельца в системах,
        /// НЕ GameObject</summary>
        public object Owner;  // может быть IAbilityOwner, ICombatActor и т.п.

        /// <summary>Позиция / направление цели.</summary>
        public Vector3 TargetPoint;
        public Vector3 Direction;

        /// <summary>Номер слота способности.</summary>
        public int SlotIndex;

        /// <summary>Поле зрения (для projectile abilities, если нужно)</summary>
        public float Yaw;
        public float Pitch;

        public AbilityContext(
            object owner,
            Vector3 targetPoint,
            Vector3 direction,
            int slotIndex,
            float yaw,
            float pitch
        )
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
