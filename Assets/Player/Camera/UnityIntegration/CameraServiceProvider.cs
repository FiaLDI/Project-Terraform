using UnityEngine;
using Features.Camera.Domain;
using Features.Camera.Application;

namespace Features.Camera.UnityIntegration
{
    /// <summary>
    /// ������ ���������� ��������� ������.
    /// ���� �� ������ / ����.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class CameraServiceProvider : MonoBehaviour
    {
        public static ICameraControlService Control { get; private set; }
        public static ICameraRuntimeService Runtime { get; private set; }

        private void Awake()
        {
            Debug.Log($"[camera-fix] CameraServiceProvider Awake | Control NULL? {Control == null}");

            if (Control == null)
            {
                Control = new CameraControlService();
                Debug.Log($"[camera-fix] Control CREATED | hash={Control.GetHashCode()}");
            }

            if (Runtime == null)
            {
                Runtime = new CameraRuntimeService();
                Debug.Log($"[camera-fix] Runtime CREATED");
            }
        }
    }
}
