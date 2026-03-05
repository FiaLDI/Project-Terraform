using UnityEngine;
using Features.Buffs.Domain;
using Features.Weapons.Data;
using Features.Tools.Data;
using Features.Items.Domain;

namespace Features.Items.Data
{
    [CreateAssetMenu(menuName = "Items/Item Definition")]
    public class Item : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string itemName;
        public string description;
        public Sprite icon;

        public bool isTwoHanded;

        [Header("Category")]
        public ItemCategory category;

        [Header("Stacking")]
        public bool isStackable = false;
        public int maxStackAmount = 1;

        // =============================
        // NEW SYSTEM
        // =============================

        [Header("Buff Applied When Equipped")]
        public BuffSO[] equippedBuffs;

        [Header("Upgrade Buffs Per Level")]
        public ItemUpgradeData[] upgrades;

        // =============================
        // VISUAL
        // =============================

        public GameObject worldPrefab;
        public GameObject equippedPrefab;
        public GameObject viewModelPrefab;

        [Header("Actions")]
        public ItemActionDefinition[] actions;
    }
}
