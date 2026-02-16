using UnityEngine;

namespace Features.Equipment.UnityIntegration
{
    /// <summary>
    /// Камера-прокси (без рендера), чтобы server-side usability (drill/scan/gun) имела cam.transform.
    /// </summary>
    public sealed class PlayerAimProxy : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera proxyCamera;

        public UnityEngine.Camera Camera => proxyCamera;

        private void Awake()
        {
            if (proxyCamera != null)
                return;

            var go = new GameObject("AimProxyCamera");
            go.transform.SetParent(transform, false);
            proxyCamera = go.AddComponent<UnityEngine.Camera>();

            proxyCamera.enabled = false;
        }

        public void SetAim(Vector3 pos, Vector3 forward)
        {
            if (proxyCamera == null)
                return;

            proxyCamera.transform.position = pos;
            if (forward.sqrMagnitude > 0.0001f)
                proxyCamera.transform.forward = forward.normalized;
        }
    }
}
