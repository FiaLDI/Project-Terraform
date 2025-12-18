// Features/Camera/Domain/ICameraService.cs
using UnityEngine;

namespace Features.Camera.Domain
{
    public interface ICameraService
    {
        /// <summary>
        /// Текущее состояние камеры: yaw/pitch/FPS/TPS/blend.
        /// </summary>
        PlayerCameraState State { get; }

        /// <summary>
        /// Применить ввод мыши (yaw/pitch).
        /// </summary>
        void SetLookInput(Vector2 lookInput, float sensitivity, float deltaTime);

        /// <summary>
        /// Начать плавное переключение FPS/TPS.
        /// </summary>
        void SwitchView();

        /// <summary>
        /// Обновление blend (плавного перехода).
        /// Вызывается 1 раз в LateUpdate адаптера.
        /// </summary>
        void UpdateTransition(float deltaTime);

        /// <summary>
        /// Рассчитать TPS дистанцию с учётом коллизий.
        /// </summary>
        float ComputeTpsDistance(
            Vector3 pivotPos,
            Vector3 targetTpsPos,
            LayerMask mask,
            float collisionRadius,
            float minDistance
        );

        /// <summary>
        /// Получить итоговую позицию камеры.
        /// </summary>
        Vector3 GetCameraPosition(Transform pivot, float distance);

        /// <summary>
        /// Получить итоговую ориентацию камеры.
        /// </summary>
        Quaternion GetCameraRotation(Transform pivot);
    }
}
