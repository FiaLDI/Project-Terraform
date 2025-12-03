using UnityEngine;
using Features.Passives.Domain;
using Features.Passives.Application;

namespace Features.Passives.UnityIntegration
{

    [DefaultExecutionOrder(-50)]
    public class PassiveSystem : MonoBehaviour
    {
        [Header("Equipped Passives (debug only)")]
        public PassiveSO[] equipped;

        private PassiveService _service;

        private void Awake()
        {
            _service = new PassiveService(gameObject);
        }

        private void OnDisable()
        {
            _service?.DeactivateAll();
        }

        // -------------------------------------------
        // NEW CORRECT BEHAVIOR
        // -------------------------------------------
        public void SetPassives(PassiveSO[] passives)
        {
            if (_service == null)
                _service = new PassiveService(gameObject);

            _service.DeactivateAll();
            equipped = passives;

            if (equipped != null && equipped.Length > 0)
                _service.ActivateAll(equipped);
        }

    }
}
