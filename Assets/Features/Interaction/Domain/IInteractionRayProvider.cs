using UnityEngine;

namespace Features.Interaction.Domain
{
    public interface IInteractionRayProvider
    {
        /// <summary>
        /// Возвращает рэй из камеры игрока.
        /// Это НЕ Raycast — только геометрия луча.
        /// </summary>
        Ray GetRay();

        /// <summary>
        /// Максимальная дистанция рейкаста.
        /// </summary>
        float MaxDistance { get; }
    }
}
