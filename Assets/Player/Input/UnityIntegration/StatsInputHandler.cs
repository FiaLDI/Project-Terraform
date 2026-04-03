using UnityEngine;
using UnityEngine.InputSystem;
using Features.Player;

namespace Features.Stats.UnityIntegration
{
    public sealed class StatsInputHandler :
        MonoBehaviour,
        IInputContextConsumer
    {
        private PlayerInputContext input;
        private StatsScreen statsScreen;

        private InputAction togglePlayer;
        private InputAction toggleUI;

        private bool subscribed;
        private StatsDebugStreamer streamer;

        // ======================================================
        // INPUT BIND
        // ======================================================

        public void BindInput(PlayerInputContext ctx)
        {
            if (input == ctx)
                return;

            if (input != null)
                UnbindInput(input);

            input = ctx;

            if (input == null)
                return;

            streamer = GetComponent<LocalPlayerController>().
                BoundPlayer.GetComponent<StatsDebugStreamer>();

            statsScreen = UIRegistry.I?.Get<StatsScreen>();

            if (statsScreen == null)
            {
                Debug.LogError("[StatsInputHandler] StatsScreen not found");
                return;
            }

            togglePlayer = input.Actions.Player.FindAction("ToggleStats", true);
            toggleUI = input.Actions.UI.FindAction("ToggleStats", true);

            togglePlayer.performed += OnToggle;
            toggleUI.performed += OnToggle;

            togglePlayer.Enable();
            toggleUI.Enable();

            subscribed = true;
        }

        public void UnbindInput(PlayerInputContext ctx)
        {
            if (!subscribed || input != ctx)
                return;

            if (togglePlayer != null)
            {
                togglePlayer.performed -= OnToggle;
                togglePlayer.Disable();
                togglePlayer = null;
            }

            if (toggleUI != null)
            {
                toggleUI.performed -= OnToggle;
                toggleUI.Disable();
                toggleUI = null;
            }

            input = null;
            subscribed = false;
        }

        // ======================================================
        // ACTION
        // ======================================================

        private void OnToggle(InputAction.CallbackContext _)
        {
            if (UIStackManager.I == null)
                return;

            if (UIStackManager.I.HasScreens)
            {
                var top = UIStackManager.I.Peek();

                if (top is StatsScreen)
                {
                    UIStackManager.I.Pop();

                    streamer?.StopStreaming();

                    return;
                }

                return;
            }

            statsScreen.Open();

            streamer?.StartStreaming();
        }
    }
}
