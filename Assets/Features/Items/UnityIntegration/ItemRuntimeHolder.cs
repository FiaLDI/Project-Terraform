using UnityEngine;
using Features.Items.Domain;

namespace Features.Items.UnityIntegration
{
    public class ItemRuntimeHolder : MonoBehaviour
    {
        public ItemInstance Instance { get; private set; }

        public void SetInstance(ItemInstance inst)
        {
            Instance = inst;
        }
    }
}
