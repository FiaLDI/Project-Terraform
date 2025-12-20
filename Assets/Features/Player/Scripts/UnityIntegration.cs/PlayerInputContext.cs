using UnityEngine;
using UnityEngine.InputSystem;

namespace Features.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputContext : MonoBehaviour
    {
        public PlayerInput PlayerInput { get; private set; }
        public GameInput Actions { get; private set; }

        private void Awake()
        {
            PlayerInput = GetComponent<PlayerInput>();

            Actions = new GameInput(PlayerInput.actions);

            PlayerInput.ActivateInput();
        }
    }

}
