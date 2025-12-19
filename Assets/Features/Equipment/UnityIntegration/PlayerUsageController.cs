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

        private bool subscribed;

        // ======================================================
        // INPUT BIND
        // ======================================================

        public void BindInput(PlayerInputContext ctx)
        {
            if (input == ctx)
                return;

            Unsubscribe();

            input = ctx;

            if (input == null)
            {
                Debug.LogError(
                    $"{nameof(PlayerUsageController)}: BindInput with NULL",
                    this);
                return;
            }

            Subscribe();
        }

        private void OnEnable()
        {
            if (input != null)
                Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        // ======================================================
        // SUBSCRIBE
        // ======================================================

        private void Subscribe()
        {
            if (subscribed || input == null)
                return;

            var p = input.Actions.Player;

            p.FindAction("Use").performed += OnPrimaryStart;
            p.FindAction("Use").canceled += OnPrimaryStop;

            p.FindAction("SecondaryUse").performed += OnSecondaryStart;
            p.FindAction("SecondaryUse").canceled += OnSecondaryStop;

            p.FindAction("Reload").performed += OnReload;

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || input == null)
                return;

            var p = input.Actions.Player;

            p.FindAction("Use").performed -= OnPrimaryStart;
            p.FindAction("Use").canceled -= OnPrimaryStop;

            p.FindAction("SecondaryUse").performed -= OnSecondaryStart;
            p.FindAction("SecondaryUse").canceled -= OnSecondaryStop;

            p.FindAction("Reload").performed -= OnReload;

            subscribed = false;
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
            rightHand?.OnUsePrimary_Start();
        }

        private void OnPrimaryStop(InputAction.CallbackContext _)
        {
            usingPrimary = false;
            rightHand?.OnUsePrimary_Stop();
        }

        // ======================================================
        // SECONDARY
        // ======================================================

        private void OnSecondaryStart(InputAction.CallbackContext _)
        {
            usingSecondary = true;
            rightHand?.OnUseSecondary_Start();
        }

        private void OnSecondaryStop(InputAction.CallbackContext _)
        {
            usingSecondary = false;
            rightHand?.OnUseSecondary_Stop();
        }

        // ======================================================
        // RELOAD
        // ======================================================

        private void OnReload(InputAction.CallbackContext _)
        {
            if (rightHand is IReloadable reloadable)
                reloadable.OnReloadPressed();
        }

        // ======================================================
        // UPDATE (HOLD)
        // ======================================================

        private void Update()
        {
            if (usingPrimary)
                rightHand?.OnUsePrimary_Hold();

            if (usingSecondary)
                rightHand?.OnUseSecondary_Hold();
        }
    }
}
