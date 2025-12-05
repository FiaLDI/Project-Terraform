using UnityEngine;
using Features.Energy.Domain;

public class GlobalEnergyCostProvider : MonoBehaviour, IEnergyCostModifier
{
    public static GlobalEnergyCostProvider I { get; private set; }

    [Range(0.1f, 3f)]
    public float globalMultiplier = 1f;

    private void Awake()
    {
        I = this;
    }

    public float ModifyCost(float baseCost)
    {
        return globalMultiplier;
    }
}
