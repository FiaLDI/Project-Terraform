using UnityEngine;
using Features.Passives.Domain;
using Features.Passives.Application;

namespace Features.Passives.UnityIntegration
{
    public class PassiveSystem : MonoBehaviour
    {
        [Header("Equipped Passives")]
        public PassiveSO[] equipped;

        private PassiveService _service;

        private void Awake()
        {
            _service = new PassiveService(gameObject);
        }

        private void OnEnable()
        {
            if (_service == null)
                _service = new PassiveService(gameObject);

            if (equipped != null)
                _service.ActivateAll(equipped);
        }

        private void OnDisable()
        {
            _service?.DeactivateAll();
        }

        public void SetPassives(PassiveSO[] passives)
        {
            _service.DeactivateAll();
            equipped = passives;
            if (equipped != null)
                _service.ActivateAll(equipped);
        }
    }
}
