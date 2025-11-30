using System.Collections.Generic;
using UnityEngine;
using Features.Passives.Domain;

namespace Features.Passives.Application
{
    public class PassiveService
    {
        private readonly GameObject _owner;
        private readonly List<PassiveSO> _active = new();

        public IReadOnlyList<PassiveSO> Active => _active;

        public PassiveService(GameObject owner)
        {
            _owner = owner;
        }

        public void Activate(PassiveSO passive)
        {
            if (passive == null || _active.Contains(passive))
                return;

            _active.Add(passive);
            passive.Apply(_owner);
        }

        public void Deactivate(PassiveSO passive)
        {
            if (passive == null || !_active.Contains(passive))
                return;

            _active.Remove(passive);
            passive.Remove(_owner);
        }

        public void ActivateAll(IEnumerable<PassiveSO> passives)
        {
            foreach (var p in passives)
            {
                Activate(p);
            }
        }

        public void DeactivateAll()
        {
            foreach (var p in _active)
            {
                p.Remove(_owner);
            }
            _active.Clear();
        }
    }
}
