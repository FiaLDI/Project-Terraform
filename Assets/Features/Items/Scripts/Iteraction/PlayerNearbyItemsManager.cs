using System.Collections.Generic;
using UnityEngine;

public class NearbyInteractables : MonoBehaviour
{
    public static NearbyInteractables instance;

    private List<ItemObject> nearby = new();

    private void Awake() => instance = this;

    public void Register(ItemObject io)
    {
        // НЕ регистрировать предметы которые не world (в руках/в UI)
        if (io != null && io.isWorldObject && !nearby.Contains(io))
            nearby.Add(io);
    }

    public void Unregister(ItemObject io)
    {
        if (io != null)
            nearby.Remove(io);
    }

    public ItemObject GetBestItem(Camera cam)
    {
        // очистка лишних null
        for (int i = nearby.Count - 1; i >= 0; i--)
        {
            if (nearby[i] == null || !nearby[i].isWorldObject)
            {
                nearby.RemoveAt(i);
                continue;
            }
        }

        if (nearby.Count == 0) return null;

        ItemObject best = null;
        float bestScore = -999;

        foreach (var io in nearby)
        {
            // защита что предмет не у игрока в иерархии (в руках)
            if (io.transform.IsChildOf(cam.transform.parent))
                continue;

            Vector3 dir = (io.transform.position - cam.transform.position).normalized;
            float dot = Vector3.Dot(cam.transform.forward, dir);

            // только те, которые перед игроком
            if (dot < 0.5f) continue;

            float dist = Vector3.Distance(cam.transform.position, io.transform.position);
            float score = dot * 2 - dist * 0.2f;

            if (score > bestScore)
            {
                bestScore = score;
                best = io;
            }
        }

        return best;
    }
}
