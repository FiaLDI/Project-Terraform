using Features.Multiplayer.UI;
using Features.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Features.Player.UnityIntegration
{
    public sealed class OnlinePlayersInputHandler :
        MonoBehaviour,
        IInputContextConsumer
    {
        private PlayerInputContext input;
        private InputAction togglePlayers;
        private bool subscribed;

        public void BindInput(PlayerInputContext ctx)
        {
            if (input == ctx)
                return;

            if (input != null)
                UnbindInput(input);

            input = ctx;
            if (input == null)
                return;

            togglePlayers = input.Actions.Player.FindAction("OnlinePlayers", true);
            togglePlayers.performed += OnToggle;
            togglePlayers.Enable();

            subscribed = true;
        }

        public void UnbindInput(PlayerInputContext ctx)
        {
            if (!subscribed || input != ctx)
                return;

            if (togglePlayers != null)
            {
                togglePlayers.performed -= OnToggle;
                togglePlayers.Disable();
                togglePlayers = null;
            }

            input = null;
            subscribed = false;
        }

        private void OnToggle(InputAction.CallbackContext _)
        {
            var screen = OnlinePlayersScreen.Resolve();
            if (screen == null)
            {
                Debug.LogWarning("[OnlinePlayersInputHandler] OnlinePlayersScreen not found.");
                return;
            }

            if (UIStackManager.I != null && UIStackManager.I.IsTop<OnlinePlayersScreen>())
            {
                screen.Close();
                return;
            }

            screen.Open();
        }
    }
}
