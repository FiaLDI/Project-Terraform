using UnityEngine;
using Features.Camera.Domain;
using Features.Camera.Application;

namespace Features.Camera.UnityIntegration
{
    /// <summary>
    /// ЕДИНЫЙ глобальный провайдер сервисов камеры:
    /// - Control: логика yaw/pitch, fps/tps, blend.
    /// - Runtime: ссылка на активную Unity-камеру, FOV, shake.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class CameraServiceProvider : MonoBehaviour
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
