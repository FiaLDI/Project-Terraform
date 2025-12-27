using System.Collections.Generic;
using Features.Interaction.Domain;
using Features.Items.UnityIntegration;
using UnityEngine;

public class NearbyInteractables : MonoBehaviour, INearbyInteractables
{
    [Header("Tuning")]
    [SerializeField] private float maxDistance = 3.0f;
    [SerializeField] private float maxAngle = 45f;

    private readonly List<WorldItemNetwork> items = new();

    public WorldItemNetwork GetBestItem(Camera cam)
    {
        if (cam == null)
            return null;

        WorldItemNetwork best = null;
        float bestScore = float.MaxValue;

        Vector3 camPos = cam.transform.position;
        Vector3 camForward = cam.transform.forward;

        foreach (var item in items)
        {
            if (item == null)
                continue;

            if (!item.IsPickupAvailable)
            {
                continue;
            }

            Vector3 toItem = item.transform.position - camPos;
            float distance = toItem.magnitude;
            float angle = Vector3.Angle(camForward, toItem);

            if (distance > maxDistance)
            {
                continue;
            }

            if (angle > maxAngle)
            {
                continue;
            }

            float score = distance + angle * 0.03f;
            if (score < bestScore)
            {
                bestScore = score;
                best = item;
            }
        }

        return best;
    }

    public void Register(WorldItemNetwork item)
    {
        if (item == null)
            return;

        if (!items.Contains(item))
            items.Add(item);
    }

    public void Unregister(WorldItemNetwork item)
    {
        if (item == null)
            return;

        items.Remove(item);
    }
}
