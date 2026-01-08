using System.Collections.Generic;
using UnityEngine;
using Features.Passives.Domain;

namespace Features.Passives.Application
{
    public sealed class PassiveService
    {
        private readonly GameObject owner;
        private readonly List<PassiveSO> active = new();

        public IReadOnlyList<PassiveSO> Active => active;

        public PassiveService(GameObject owner)
        {
            this.owner = owner;
        }

        public void Activate(PassiveSO passive)
        {
            Debug.Log($"[PASSIVES] Activate {passive.name}", owner);

            if (passive == null || active.Contains(passive))
                return;
            
            active.Add(passive);
            passive.Apply(owner);
        }

        public void Deactivate(PassiveSO passive)
        {
            if (passive == null || !active.Contains(passive))
                return;

            active.Remove(passive);
            passive.Remove(owner);
        }

        public void ActivateAll(IEnumerable<PassiveSO> passives)
        {
            if (passives == null)
                return;

            foreach (var p in passives)
                Activate(p);
        }

        public void DeactivateAll()
        {
            for (int i = active.Count - 1; i >= 0; i--)
                active[i].Remove(owner);

            active.Clear();
        }
    }
}
