using UnityEngine;
using Features.Items.Domain;
using Features.Buffs.Domain;

namespace Features.Items.UnityIntegration
{
    public class ItemRuntimeHolder : MonoBehaviour
    {
        public ItemInstance Instance { get; private set; }
        public ItemRuntimeSource Source { get; private set; }

        public void SetInstance(ItemInstance inst, IBuffSource owner)
        {
            Instance = inst;

            Source =
                GetComponent<ItemRuntimeSource>() ??
                gameObject.AddComponent<ItemRuntimeSource>();

            Source.Init(inst, owner);
        }
    }
}
