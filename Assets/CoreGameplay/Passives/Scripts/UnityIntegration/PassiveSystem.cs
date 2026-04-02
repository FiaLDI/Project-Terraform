using UnityEngine;
using Features.Passives.Domain;
using Features.Passives.Application;
using FishNet.Object;
using System.Collections.Generic;

namespace Features.Passives.UnityIntegration
{
    public sealed class PassiveSystem : NetworkBehaviour
    {
        private PassiveService service;

        public IReadOnlyList<AbilityModifierSO> GetCachedModifiers()
        {
            return service.CachedModifiers;
        }

        public override void OnStartServer()
        {
            service = new PassiveService(GetComponent<StatsBuffTarget>());
        }

        [Server]
        public void SetPassives(IEnumerable<PassiveSO> passives)
        {
            service.Set(passives);
        }

        [Server]
        public void ResetPassives()
        {
            service.ClearAll();
        }
    }
}
