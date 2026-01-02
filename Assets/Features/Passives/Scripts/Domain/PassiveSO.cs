using UnityEngine;
using System.Collections;
using Features.Buffs.Domain;
using Features.Buffs.Application;

namespace Features.Passives.Domain
{
    public abstract class PassiveSO : ScriptableObject
    {
        public string id;
        public string displayName;
        public Sprite icon;

        // =====================================================
        // APPLY
        // =====================================================

        public void Apply(GameObject owner)
        {
            Debug.Log($"[PASSIVES] PassiveSO.Apply {name}", this);
            if (owner == null)
                return;

            ApplyInternal(owner);
        }

        // =====================================================
        // REMOVE
        // =====================================================

        public void Remove(GameObject owner)
        {
            if (owner == null)
                return;

            RemoveInternal(owner);
        }
       
        // =====================================================
        // TO IMPLEMENT
        // =====================================================

        protected abstract void ApplyInternal(GameObject owner);
        protected abstract void RemoveInternal(GameObject owner);
    }
}
