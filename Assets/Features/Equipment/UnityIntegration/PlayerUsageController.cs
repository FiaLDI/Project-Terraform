using UnityEngine;
using UnityEngine.InputSystem;
using Features.Equipment.Domain;
using Features.Player;
using Features.Game;

namespace Features.Equipment.UnityIntegration
{
    /// <summary>
    /// Глобальный контроллер использования (стрельба, вторичное, перезарядка),
    /// который читает input и прокидывает его на PlayerUsageNetAdapter локального игрока.
    /// </summary>
    public class PlayerUsageController : MonoBehaviour, IInputContextConsumer
    {
        private PlayerInputContext input;

        private bool usingPrimary;
        private bool usingSecondary;
        private bool bound;

        // Локальный адаптер с игрока, берём через BootstrapRoot
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

            usingPrimary   = false;
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

            p.FindAction("Use").performed        += OnPrimaryStart;
            p.FindAction("Use").canceled         += OnPrimaryStop;

            p.FindAction("SecondaryUse").performed += OnSecondaryStart;
            p.FindAction("SecondaryUse").canceled  += OnSecondaryStop;

            p.FindAction("Reload").performed     += OnReload;
        }

        private void UnbindActions()
        {
            if (input == null)
                return;

            var p = input.Actions.Player;

            p.FindAction("Use").performed        -= OnPrimaryStart;
            p.FindAction("Use").canceled         -= OnPrimaryStop;

            p.FindAction("SecondaryUse").performed -= OnSecondaryStart;
            p.FindAction("SecondaryUse").canceled  -= OnSecondaryStop;

            p.FindAction("Reload").performed     -= OnReload;

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
                net.PrimaryStart();
            else
                Debug.LogWarning("[PlayerUsageController] PrimaryStart: Net adapter not found on LocalPlayer", this);
        }

        private void OnPrimaryStop(InputAction.CallbackContext _)
        {
            usingPrimary = false;

            var net = Net;
            if (net != null)
                net.PrimaryStop();
        }

        // ======================================================
        // SECONDARY
        // ======================================================

        private void OnSecondaryStart(InputAction.CallbackContext _)
        {
            usingSecondary = true;

            var net = Net;
            if (net != null)
                net.SecondaryStart();
            else
                Debug.LogWarning("[PlayerUsageController] SecondaryStart: Net adapter not found on LocalPlayer", this);
        }

        private void OnSecondaryStop(InputAction.CallbackContext _)
        {
            usingSecondary = false;

            var net = Net;
            if (net != null)
                net.SecondaryStop();
        }

        // ======================================================
        // RELOAD
        // ======================================================

        private void OnReload(InputAction.CallbackContext _)
        {
            var net = Net;
            if (net != null)
                net.Reload();
            else
                Debug.LogWarning("[PlayerUsageController] Reload: Net adapter not found on LocalPlayer", this);
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

        private static bool TryGetAim(out Vector3 pos, out Vector3 forward)
        {
            var cam = UnityEngine.Camera.main;
            if (cam == null)
            {
                pos     = default;
                forward = Vector3.forward;
                return false;
            }

            pos     = cam.transform.position;
            forward = cam.transform.forward;
            return true;
        }
    }
}
