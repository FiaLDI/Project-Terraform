using UnityEngine;
using UnityEngine.InputSystem;
using Features.Equipment.Domain;
using Features.Player;
using Features.Game;

namespace Features.Equipment.UnityIntegration
{
    /// <summary>
    /// Читает input и прокидывает действия предметов в PlayerUsageNetAdapter.
    /// </summary>
    public class PlayerUsageController : MonoBehaviour, IInputContextConsumer
    {
        private PlayerInputContext input;

        private bool usingPrimary;
        private bool usingSecondary;

        private bool bound;

        // ======================================================
        // LOCAL NET ADAPTER
        // ======================================================

        private PlayerUsageNetAdapter Net
        {
            get
            {
                var player = BootstrapRoot.I?.LocalPlayer;

                return player != null
                    ? player.GetComponent<PlayerUsageNetAdapter>()
                    : null;
            }
        }

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

            BindActions();
            bound = true;

            Debug.Log("[PlayerUsageController] BindInput OK", this);
        }

        public void UnbindInput(PlayerInputContext ctx)
        {
            if (!bound || input != ctx)
                return;

            UnbindActions();

            usingPrimary = false;
            usingSecondary = false;

            input = null;
            bound = false;

            Debug.Log("[PlayerUsageController] UnbindInput", this);
        }

        // ======================================================
        // ACTIONS
        // ======================================================

        private void BindActions()
        {
            var p = input.Actions.Player;

            Enable(p, "Use", "SecondaryUse", "Reload");

            p.FindAction("Use").performed += OnPrimaryStart;
            p.FindAction("Use").canceled += OnPrimaryStop;

            p.FindAction("SecondaryUse").performed += OnSecondaryStart;
            p.FindAction("SecondaryUse").canceled += OnSecondaryStop;

            p.FindAction("Reload").performed += OnReload;
        }

        private void UnbindActions()
        {
            if (input == null)
                return;

            var p = input.Actions.Player;

            p.FindAction("Use").performed -= OnPrimaryStart;
            p.FindAction("Use").canceled -= OnPrimaryStop;

            p.FindAction("SecondaryUse").performed -= OnSecondaryStart;
            p.FindAction("SecondaryUse").canceled -= OnSecondaryStop;

            p.FindAction("Reload").performed -= OnReload;

            Disable(p, "Use", "SecondaryUse", "Reload");
        }

        // ======================================================
        // PRIMARY
        // ======================================================

        private void OnPrimaryStart(InputAction.CallbackContext _)
        {
            usingPrimary = true;

            var net = Net;

            if (net != null)
                net.ActionStart(ItemActionType.Primary);
            else
                Debug.LogWarning("[PlayerUsageController] PrimaryStart: Net adapter not found", this);
        }

        private void OnPrimaryStop(InputAction.CallbackContext _)
        {
            usingPrimary = false;

            var net = Net;

            if (net != null)
                net.ActionStop(ItemActionType.Primary);
        }

        // ======================================================
        // SECONDARY
        // ======================================================

        private void OnSecondaryStart(InputAction.CallbackContext _)
        {
            usingSecondary = true;

            var net = Net;

            if (net != null)
                net.ActionStart(ItemActionType.Secondary);
        }

        private void OnSecondaryStop(InputAction.CallbackContext _)
        {
            usingSecondary = false;

            var net = Net;

            if (net != null)
                net.ActionStop(ItemActionType.Secondary);
        }

        // ======================================================
        // RELOAD
        // ======================================================

        private void OnReload(InputAction.CallbackContext _)
        {
            var net = Net;

            if (net != null)
                net.ActionStart(ItemActionType.Reload);
        }

        // ======================================================
        // HELPERS
        // ======================================================

        private static void Enable(InputActionMap map, params string[] names)
        {
            foreach (var n in names)
                map.FindAction(n, true).Enable();
        }

        private static void Disable(InputActionMap map, params string[] names)
        {
            foreach (var n in names)
                map.FindAction(n, true).Disable();
        }
    }
}