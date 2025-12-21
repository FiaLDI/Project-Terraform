using UnityEngine;
using UnityEngine.InputSystem;

namespace Features.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputContext : MonoBehaviour
    {
        public PlayerInput PlayerInput { get; private set; }
        public GameInput Actions { get; private set; }

        private const string PLAYER_MAP = "Player";

        private void Awake()
        {
            PlayerInput = GetComponent<PlayerInput>();
            Actions = new GameInput(PlayerInput.actions);
        }

        // ======================================================
        // ENABLE
        // ======================================================

        public void Enable()
        {
            if (!PlayerInput.enabled)
                PlayerInput.enabled = true;

            PlayerInput.ActivateInput();

            if (PlayerInput.currentActionMap == null ||
                PlayerInput.currentActionMap.name != PLAYER_MAP)
            {
                PlayerInput.SwitchCurrentActionMap(PLAYER_MAP);
            }
        }

        // ======================================================
        // DISABLE
        // ======================================================

        public void Disable()
        {
            if (!PlayerInput.enabled)
                return;

            PlayerInput.DeactivateInput();
        }
    }
}
