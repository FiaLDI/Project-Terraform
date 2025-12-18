
using Features.Items.Data;

namespace Features.Items.Application
{
    public class ItemUpgradeService
    {
        public ItemUpgradeData GetCurrentUpgrade(Item item, int level)
        {
            if (level < 0 || level >= item.upgrades.Length)
                return null;

            return item.upgrades[level];
        }

        public ItemUpgradeData GetNextUpgrade(Item item, int level)
        {
            int next = level + 1;
            if (next < 0 || next >= item.upgrades.Length)
                return null;

            return item.upgrades[next];
        }
    }
}
