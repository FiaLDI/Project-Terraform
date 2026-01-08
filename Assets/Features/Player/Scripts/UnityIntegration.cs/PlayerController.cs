using UnityEngine;
using Features.Abilities.Application;
using Features.Camera.UnityIntegration;

namespace Features.Player.UnityIntegration
{
    // Больше не IInputContextConsumer!
    public class PlayerController : MonoBehaviour
    {
        [Header("Core References (Optional Facade)")]
        [SerializeField] private PlayerMovementNetAdapter movementNet;
        [SerializeField] private PlayerCameraController playerCameraController;
        [SerializeField] private AbilityCasterNetAdapter abilityCasterNet;

        private void Awake()
        {
            // Можно оставить для валидации, что на префабе всё есть
            if (movementNet == null) movementNet = GetComponent<PlayerMovementNetAdapter>();
            if (playerCameraController == null) playerCameraController = GetComponent<PlayerCameraController>();
            if (abilityCasterNet == null) abilityCasterNet = GetComponent<AbilityCasterNetAdapter>();
        }
    }
}
