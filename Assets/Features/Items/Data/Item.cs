using UnityEngine;
using Features.Weapons.Data;
using Features.Items.Domain;
using Features.Tools.Data;

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

        [Header("Upgrades / Stats")]
        public ItemStat[] baseStats;
        public ItemUpgradeData[] upgrades;

        [Header("World")]
        public GameObject worldPrefab;

        [Header("Equipped")]
        public GameObject equippedPrefab;

        [Header("Feature Configs - Optional Feature-specific configs (they may be null) ")]
        public WeaponConfig weaponConfig;
        public ToolConfig toolConfig;
        public ScannerConfig scannerConfig;
        public ThrowableConfig throwableConfig;
    }
}
