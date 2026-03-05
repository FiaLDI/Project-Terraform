using UnityEngine;
using FishNet.Object;
using Features.Camera.UnityIntegration;

namespace Features.Player.UnityIntegration
{
    public class PlayerController : NetworkBehaviour
    {
        [Header("Core References")]
        [SerializeField] private PlayerNetworkController networkController;
        [SerializeField] private PlayerCameraController playerCameraController;
        [SerializeField] private AbilityCasterNetAdapter abilityCasterNet;
        [SerializeField] private PlayerVisualController visualController;

        private void Awake()
        {
            // Можно оставить для валидации, что на префабе всё есть
            if (networkController == null) networkController = GetComponent<PlayerNetworkController>();
            if (playerCameraController == null) playerCameraController = GetComponent<PlayerCameraController>();
            if (abilityCasterNet == null) abilityCasterNet = GetComponent<AbilityCasterNetAdapter>();
            if (visualController == null)
                visualController = GetComponent<PlayerVisualController>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!IsOwner)
                return;

            var input = FindObjectOfType<MovementInputHandler>();
            var net = GetComponent<PlayerNetworkController>();

            net.InjectInput(input);

            if (CameraRegistry.Instance != null)
            {
                CameraRegistry.Instance.OnFPSModeChanged += OnFPSModeChanged;

                OnFPSModeChanged(CameraRegistry.Instance.IsFPSActive);
            }
        }

        private void OnFPSModeChanged(bool isFPS)
        {
            visualController?.SetLocalModelVisible(!isFPS);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            if (IsOwner)
            {
                playerCameraController?.SetLocal(false);
            }
            if (CameraRegistry.Instance != null)
                CameraRegistry.Instance.OnFPSModeChanged -= OnFPSModeChanged;
        }
    }
}
