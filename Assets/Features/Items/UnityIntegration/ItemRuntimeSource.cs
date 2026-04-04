using UnityEngine;
using Features.Buffs.Domain;
using Features.Items.Domain;

namespace Features.Items.UnityIntegration
{
    /// <summary>
    /// Уникальный источник баффов предмета.
    /// </summary>
    public sealed class ItemRuntimeSource : MonoBehaviour, IBuffSource
    {
        public ItemInstance Instance { get; private set; }

        private IBuffSource owner;
        public Transform Muzzle { get; private set; }

        public void Init(ItemInstance inst, IBuffSource ownerSource)
        {
            Instance = inst;
            owner = ownerSource;
        }

        public GameObject GameObject => gameObject;

        public Transform Transform => transform;

        public IBuffSource OwnerSource => owner;
    }
}