using UnityEngine;
using FishNet.Object;

namespace Features.Player.UnityIntegration
{
    public class PlayerController : NetworkBehaviour
    {
        [Header("Core References")]
        [SerializeField] private PlayerNetworkController networkController;
        [SerializeField] private PlayerCameraController playerCameraController;
        [SerializeField] private AbilityCasterNetAdapter abilityCasterNet;

        private void Awake()
        {
            // Можно оставить для валидации, что на префабе всё есть
            if (networkController == null) networkController = GetComponent<PlayerNetworkController>();
            if (playerCameraController == null) playerCameraController = GetComponent<PlayerCameraController>();
            if (abilityCasterNet == null) abilityCasterNet = GetComponent<AbilityCasterNetAdapter>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!IsOwner)
                return;

            var input = FindObjectOfType<MovementInputHandler>();
            var net = GetComponent<PlayerNetworkController>();

            net.InjectInput(input);
        }


        public override void OnStopClient()
        {
            base.OnStopClient();

            if (IsOwner)
            {
                playerCameraController?.SetLocal(false);
            }
        }
    }
}
