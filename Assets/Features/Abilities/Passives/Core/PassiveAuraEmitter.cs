using UnityEngine;
using System.Collections.Generic;

public class PassiveAuraEmitter : MonoBehaviour
{
    public float radius = 15f;
    public float energyReductionPercent = 20f;

    private readonly HashSet<EnergyConsumer> affected = new();

    private void Update()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        HashSet<EnergyConsumer> newAffected = new();

        foreach (var h in hits)
        {
            var device = h.GetComponent<EnergyConsumer>();
            if (device == null) continue;

            newAffected.Add(device);

            if (!affected.Contains(device))
            {
                device.energyUseMultiplier *= (1f - energyReductionPercent / 100f);
            }
        }

        // remove device that exited radius
        foreach (var old in affected)
        {
            if (!newAffected.Contains(old))
            {
                old.energyUseMultiplier /= (1f - energyReductionPercent / 100f);
            }
        }

        affected.Clear();
        foreach (var dev in newAffected)
            affected.Add(dev);
    }

    private void OnDestroy()
    {
        foreach (var dev in affected)
        {
            dev.energyUseMultiplier /= (1f - energyReductionPercent / 100f);
        }
    }
}
