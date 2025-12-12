using UnityEngine;
using Features.Equipment.Domain;

namespace Features.Equipment.UnityIntegration
{
    /// <summary>
    /// Управляет использованием предметов в руках.
    /// Use          — primary (правая рука / двуручное)
    /// SecondaryUse — secondary (ADS / alt)
    /// </summary>
    public class PlayerUsageController : MonoBehaviour
    {
        public static bool InteractionLocked = false;

        private InputSystem_Actions input;

        private IUsable rightHand;
        private IUsable leftHand;
        private bool isTwoHanded;

        private bool usingPrimary;
        private bool usingSecondary;

        private void Awake()
        {
            input = new InputSystem_Actions();

            // === PRIMARY USE ===
            input.Player.Use.performed += _ => StartPrimary();
            input.Player.Use.canceled += _ => StopPrimary();

            // === SECONDARY USE ===
            input.Player.SecondaryUse.performed += _ => StartSecondary();
            input.Player.SecondaryUse.canceled += _ => StopSecondary();
        }

        private void OnEnable() => input.Enable();
        private void OnDisable() => input.Disable();

        // ====================================================================
        // CALLED BY EquipmentManager
        // ====================================================================

        public void OnHandsUpdated(IUsable left, IUsable right, bool twoHanded)
        {
            leftHand = left;
            rightHand = right;
            isTwoHanded = twoHanded;
        }

        // ====================================================================
        // PRIMARY
        // ====================================================================

        private void StartPrimary()
        {
            if (InteractionLocked)
                return;

            usingPrimary = true;

            // Primary всегда относится к правой руке
            rightHand?.OnUsePrimary_Start();

            // Если не двуручное — левая рука может быть свободной
        }

        private void StopPrimary()
        {
            usingPrimary = false;
            rightHand?.OnUsePrimary_Stop();
        }

        // ====================================================================
        // SECONDARY
        // ====================================================================

        private void StartSecondary()
        {
            if (InteractionLocked)
                return;

            usingSecondary = true;
            rightHand?.OnUseSecondary_Start();
        }

        private void StopSecondary()
        {
            usingSecondary = false;
            rightHand?.OnUseSecondary_Stop();
        }

        // ====================================================================
        // UPDATE (HOLD)
        // ====================================================================

        private void Update()
        {
            if (InteractionLocked)
                return;

            if (usingPrimary)
                rightHand?.OnUsePrimary_Hold();

            if (usingSecondary)
                rightHand?.OnUseSecondary_Hold();
        }
    }
}
