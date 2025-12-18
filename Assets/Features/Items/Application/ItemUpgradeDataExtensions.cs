using System.Text;

public static class ItemUpgradeDataExtensions
{
    public static string ToStatsText(this ItemUpgradeData upgrade)
    {
        if (upgrade == null || upgrade.bonusStats == null || upgrade.bonusStats.Length == 0)
            return "No bonuses";

        var sb = new StringBuilder();

        foreach (var stat in upgrade.bonusStats)
        {
            string sign = stat.value >= 0 ? "+" : "";
            sb.AppendLine($"{sign}{stat.value} {StatName(stat.stat)}");
        }

        return sb.ToString();
    }

    private static string StatName(ItemStatType type)
    {
        return type switch
        {
            ItemStatType.Damage      => "Damage",
            ItemStatType.FireRate    => "Fire Rate",
            ItemStatType.Range       => "Range",
            ItemStatType.Spread      => "Accuracy",
            ItemStatType.Recoil      => "Recoil",
            ItemStatType.MiningSpeed => "Mining Speed",
            ItemStatType.ScanRange   => "Scan Range",
            ItemStatType.ScanSpeed   => "Scan Speed",
            ItemStatType.Cooldown    => "Cooldown",
            _ => type.ToString()
        };
    }
}
