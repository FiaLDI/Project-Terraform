using Features.Items.Domain;
using Features.Weapons.Data;
using Features.Weapons.Domain;
using UnityEngine;

namespace Features.Weapons.Application
{
    public static class WeaponStatCalculator
    {
        public static WeaponRuntimeStats Calculate(ItemInstance item)
        {
            var config = item.itemDefinition.weaponConfig;

            var stats = new WeaponRuntimeStats
            {
                damage = config.damage,
                fireRate = config.fireRate,
                range = config.maxDistance,
                spread = config.hipfireSpread,
                aimSpread = config.adsSpread,
                recoil = config.verticalRecoil,
                magazineSize = config.magazineSize
            };

            // === APPLY UPGRADES (LEVEL-BASED) ===
            var upgrades = item.itemDefinition.upgrades;
            if (upgrades == null)
                return stats;

            int level = Mathf.Clamp(item.level, 0, upgrades.Length);

            for (int i = 0; i < level; i++)
            {
                ApplyUpgrade(stats, upgrades[i]);
            }

            return stats;
        }


        private static void ApplyUpgrade(
        WeaponRuntimeStats stats,
        ItemUpgradeData upgrade)
            {
                if (upgrade.bonusStats == null)
                    return;

                foreach (var stat in upgrade.bonusStats)
                {
                    switch (stat.stat)
                    {
                        case ItemStatType.Damage:
                            stats.damage += stat.value;
                            break;

                        case ItemStatType.FireRate:
                            stats.fireRate += stat.value;
                            break;

                        case ItemStatType.Range:
                            stats.range += stat.value;
                            break;

                        case ItemStatType.Spread:
                            stats.spread -= stat.value;
                            stats.aimSpread -= stat.value;
                            break;

                        case ItemStatType.Recoil:
                            stats.recoil -= stat.value;
                            break;

                        default:
                            break;
                    }
                }
            }

    }
}
