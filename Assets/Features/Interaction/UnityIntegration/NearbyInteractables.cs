using System.Collections.Generic;
using Features.Interaction.Domain;
using UnityEngine;

public class NearbyInteractables : MonoBehaviour, INearbyInteractables
{
    private readonly List<NearbyItemPresenter> items = new();

    public NearbyItemPresenter GetBestItem(Camera cam)
    {
        NearbyItemPresenter best = null;
        float bestScore = float.MaxValue;

        foreach (var item in items)
        {
            if (item == null) continue;

            float dist = Vector3.Distance(
                cam.transform.position,
                item.transform.position
            );

            if (dist < bestScore)
            {
                bestScore = dist;
                best = item;
            }
        }

        return best;
    }

    public void Register(NearbyItemPresenter presenter)
    {
        if (!items.Contains(presenter))
            items.Add(presenter);
    }

    public void Unregister(NearbyItemPresenter presenter)
    {
        items.Remove(presenter);
    }
}
