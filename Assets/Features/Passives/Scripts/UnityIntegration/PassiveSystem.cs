using UnityEngine;
using Features.Passives.Domain;
using Features.Passives.Application;

namespace Features.Passives.UnityIntegration
{
    [DefaultExecutionOrder(-50)]
    public sealed class PassiveSystem : MonoBehaviour
    {
        [Header("Equipped Passives (debug only)")]
        public PassiveSO[] equipped;

        private PassiveService service;

        private void Awake()
        {
            service = new PassiveService(gameObject);
        }

        private void OnDisable()
        {
            service?.DeactivateAll();
        }

        // =====================================================
        // LOGIC (SERVER / OWNER)
        // =====================================================

        public void SetPassivesLogic(PassiveSO[] passives)
        {
            if (service == null)
                service = new PassiveService(gameObject);

            service.DeactivateAll();
            equipped = passives;

            if (equipped != null && equipped.Length > 0)
                service.ActivateAll(equipped);
        }

        // =====================================================
        // VISUALS (CLIENT ONLY)
        // =====================================================

        public void SetPassivesVisuals(PassiveSO[] passives)
        {
            equipped = passives;
            // ⚠️ сознательно НЕ вызываем Apply / Remove
        }

        // =====================================================
        // UNIFIED ENTRY (через NetAdapter)
        // =====================================================

        public void SetPassives(PassiveSO[] passives)
        {
            SetPassivesLogic(passives);
        }
    }
}
