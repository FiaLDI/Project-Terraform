using UnityEngine;
using Features.Buffs.Domain;
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

        [Header("Handling")]
        public bool isTwoHanded;

        [Tooltip("0 = none, 1 = one-hand, 2 = two-hand")]
        [Range(0, 2)]
        public int weaponPose = 0;

        [Header("Category")]
        public ItemCategory category;

        [Header("Stacking")]
        public bool isStackable = false;
        public int maxStackAmount = 1;

        [Header("Consumption")]
        public bool isConsumable = false;

        // =============================
        // BUFFS
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

        // =============================
        // HELPER
        // =============================

        public int GetWeaponPose()
        {
            if (isTwoHanded)
                return 2;

            return weaponPose;
        }
    }
}
