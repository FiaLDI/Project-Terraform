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
            if (owner == null)
                return;

            var buffSystem = owner.GetComponent<BuffSystem>();
            if (buffSystem == null)
            {
                var runner = GetCoroutineRunner(owner);
                if (runner != null)
                    runner.StartCoroutine(WaitAndApply(owner));
                return;
            }

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
        // WAIT
        // =====================================================

        private IEnumerator WaitAndApply(GameObject owner)
        {
            while (owner != null && owner.GetComponent<BuffSystem>() == null)
                yield return null;

            if (owner != null)
                ApplyInternal(owner);
        }

        private static MonoBehaviour GetCoroutineRunner(GameObject owner)
        {
            return owner.GetComponent<PlayerClassController>()
                ?? owner.GetComponent<MonoBehaviour>();
        }

        // =====================================================
        // TO IMPLEMENT
        // =====================================================

        protected abstract void ApplyInternal(GameObject owner);
        protected abstract void RemoveInternal(GameObject owner);
    }
}
