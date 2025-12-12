using Features.Items.Domain;
using Features.Tools.Domain;
using UnityEngine;

namespace Features.Tools.Application
{
    public static class ToolStatCalculator
    {
        public static ToolRuntimeStats Calculate(ItemInstance inst)
        {
            var stats = new ToolRuntimeStats();
            var def = inst.itemDefinition;

            // ------------------------
            // TOOL CONFIG
            // ------------------------
            if (def.toolConfig != null)
            {
                var cfg = def.toolConfig;
                stats.Add(ToolStat.MiningSpeed, cfg.baseMiningSpeed);
                stats.Add(ToolStat.Damage,      cfg.baseDamage);
                stats.Add(ToolStat.Range,       cfg.baseRange);
            }

            // ------------------------
            // SCANNER CONFIG
            // ------------------------
            if (def.scannerConfig != null)
            {
                var cfg = def.scannerConfig;
                stats.Add(ToolStat.ScanRange, cfg.baseScanRange);
                stats.Add(ToolStat.ScanSpeed, cfg.baseScanSpeed);
                stats.Add(ToolStat.Cooldown,  cfg.baseCooldown);
            }

            // ------------------------
            // THROWABLE CONFIG
            // ------------------------
            if (def.throwableConfig != null)
            {
                stats.Add(
                    ToolStat.ThrowForce,
                    def.throwableConfig.baseThrowForce
                );
            }

            // ------------------------
            // ITEM UPGRADES (LEVEL-BASED)
            // ------------------------
            var upgrades = def.upgrades;
            if (upgrades != null)
            {
                int level = Mathf.Clamp(inst.level, 0, upgrades.Length);

                for (int i = 0; i < level; i++)
                {
                    ApplyUpgrade(stats, upgrades[i]);
                }
            }

            return stats;
        }

        private static void ApplyUpgrade(
            ToolRuntimeStats stats,
            ItemUpgradeData upgrade)
        {
            if (upgrade.bonusStats == null)
                return;

            foreach (var stat in upgrade.bonusStats)
            {
                switch (stat.stat)
                {
                    case ItemStatType.MiningSpeed:
                        stats.Add(ToolStat.MiningSpeed, stat.value);
                        break;

                    case ItemStatType.Damage:
                        stats.Add(ToolStat.Damage, stat.value);
                        break;

                    case ItemStatType.Range:
                        stats.Add(ToolStat.Range, stat.value);
                        break;

                    case ItemStatType.ScanRange:
                        stats.Add(ToolStat.ScanRange, stat.value);
                        break;

                    case ItemStatType.ScanSpeed:
                        stats.Add(ToolStat.ScanSpeed, stat.value);
                        break;

                    case ItemStatType.Cooldown:
                        stats.Add(ToolStat.Cooldown, -stat.value);
                        break;

                    default:
                        // нерелевантные статы игнорируем
                        break;
                }
            }
        }
    }
}
