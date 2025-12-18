using System.Collections.Generic;
using Features.Items.Data;
using Features.Resources.Domain;

namespace Features.Resources.Application
{
    public class ResourceDropService
    {
        public IEnumerable<Item> RollDrops(DropEntry[] entries)
        {
            var list = new List<Item>();

            foreach (var e in entries)
            {
                if (e.item == null) continue;
                if (UnityEngine.Random.value > e.chance) continue;

                int count = UnityEngine.Random.Range(e.min, e.max + 1);
                for (int i = 0; i < count; i++)
                    list.Add(e.item);
            }

            return list;
        }
    }
}
