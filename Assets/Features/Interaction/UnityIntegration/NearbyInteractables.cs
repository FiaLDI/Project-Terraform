using System.Collections.Generic;
using Features.Interaction.Domain;
using UnityEngine;

public class NearbyInteractables : MonoBehaviour, INearbyInteractables
{
    [Header("Tuning")]
    [SerializeField] private float maxDistance = 3.0f;
    [SerializeField] private float maxAngle = 45f;

    private readonly List<NearbyItemPresenter> items = new();

    public NearbyItemPresenter GetBestItem(Camera cam)
    {
        if (cam == null)
        {
            Debug.LogWarning("[NearbyInteractables] Camera is NULL");
            return null;
        }

        NearbyItemPresenter best = null;
        float bestScore = float.MaxValue;

        Vector3 camPos = cam.transform.position;
        Vector3 camForward = cam.transform.forward;

        foreach (var item in items)
        {
            if (item == null)
                continue;

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

    // ======================================================
    // REGISTRATION
    // ======================================================

    public void Register(NearbyItemPresenter presenter)
    {
        if (presenter == null)
            return;

        if (!items.Contains(presenter))
        {
            items.Add(presenter);
        }
    }

    public void Unregister(NearbyItemPresenter presenter)
    {
        if (presenter == null)
            return;

        if (items.Remove(presenter))
        {
        }
    }
}
