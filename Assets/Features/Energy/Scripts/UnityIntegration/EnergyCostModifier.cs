using UnityEngine;
using Features.Energy.Domain;

public class EnergyCostModifier : MonoBehaviour, IEnergyCostModifier
{
    [Range(0.1f, 3f)]
    public float multiplier = 1f;

    public float ModifyCost(float baseCost)
    {
        return multiplier;
    }
}
