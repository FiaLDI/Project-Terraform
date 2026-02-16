using UnityEngine;
using Features.Camera.Domain;
using Features.Camera.Application;

namespace Features.Camera.UnityIntegration
{
    /// <summary>
    /// ЕДИНЫЙ глобальный провайдер камеры.
    /// Один на клиент / хост.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class CameraServiceProvider : MonoBehaviour
    {
        public static ICameraControlService Control { get; private set; }
        public static ICameraRuntimeService Runtime { get; private set; }

        private void Awake()
        {
            if (Control == null)
                Control = new CameraControlService();

            if (Runtime == null)
                Runtime = new CameraRuntimeService();
        }
    }
}
