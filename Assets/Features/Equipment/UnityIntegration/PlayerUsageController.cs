using UnityEngine;
using UnityEngine.InputSystem;
using Features.Equipment.Domain;
using Features.Player;

namespace Features.Equipment.UnityIntegration
{
    public class PlayerUsageController : MonoBehaviour, IInputContextConsumer
    {
        private PlayerInputContext input;

        private IUsable rightHand;
        private IUsable leftHand;
        private bool isTwoHanded;

        private bool usingPrimary;
        private bool usingSecondary;

        private bool bound;

        private PlayerUsageNetAdapter net;

        // ======================================================
        // INPUT BIND
        // ======================================================
        
        private void Awake()
        {
            net = GetComponent<PlayerUsageNetAdapter>();
        }

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
        // CALLED BY EquipmentManager
        // ======================================================

        public void OnHandsUpdated(IUsable left, IUsable right, bool twoHanded)
        {
            leftHand = left;
            rightHand = right;
            isTwoHanded = twoHanded;
        }

        // ======================================================
        // PRIMARY
        // ======================================================

        private void OnPrimaryStart(InputAction.CallbackContext _)
        {
            usingPrimary = true;

            net?.PrimaryStart();
        }

        private void OnPrimaryStop(InputAction.CallbackContext _)
        {
            usingPrimary = false;
            net?.PrimaryStop();
        }

        // ======================================================
        // SECONDARY
        // ======================================================

        private void OnSecondaryStart(InputAction.CallbackContext _)
        {
            usingSecondary = true;

            net?.SecondaryStart();
        }

        private void OnSecondaryStop(InputAction.CallbackContext _)
        {
            usingSecondary = false;
            net?.SecondaryStop();
        }

        // ======================================================
        // RELOAD
        // ======================================================

        private void OnReload(InputAction.CallbackContext _)
        {
            net?.Reload();
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
                pos = default;
                forward = Vector3.forward;
                return false;
            }

            pos = cam.transform.position;
            forward = cam.transform.forward;
            return true;
        }
    }
}
