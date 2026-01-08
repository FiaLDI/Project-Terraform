using FishNet.Serializing;
using UnityEngine;

namespace Features.Inventory.Domain
{
    public struct InventoryCommandData
    {
        public InventoryCommand Command;
        public uint WorldItemNetId;

        public InventorySection Section;
        public int Index;

        public InventorySection FromSection;
        public int FromIndex;

        public InventorySection ToSection;
        public int ToIndex;

        public int Amount;

        public Vector3 WorldPos;
        public Vector3 WorldForward;

        public string RecipeId;

        public string ItemId;
        public int PickupQuantity;
        public int PickupLevel; 
    }
}
