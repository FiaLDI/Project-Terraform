using UnityEngine;
using System;

public class PlayerEnergy : MonoBehaviour
{
    [Header("Energy")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float regenPerSecond = 8f;

    public float CurrentEnergy { get; private set; }
    public float MaxEnergy => maxEnergy;

    public event Action<float, float> OnEnergyChanged;

    private void Start()
    {
        CurrentEnergy = maxEnergy;
        Notify();
    }

    private void Update()
    {
        if (CurrentEnergy < maxEnergy)
        {
            CurrentEnergy += regenPerSecond * Time.deltaTime;
            CurrentEnergy = Mathf.Clamp(CurrentEnergy, 0, maxEnergy);
            Notify();
        }
    }

    public void SetMaxEnergy(float value, bool fill)
    {
        maxEnergy = value;

        if (fill)
            CurrentEnergy = maxEnergy;

        Notify();
    }

    public void SetRegen(float value)
    {
        regenPerSecond = value;
    }

    public bool HasEnergy(float amount)
    {
        return CurrentEnergy >= amount;
    }

    public bool TrySpend(float amount)
    {
        if (CurrentEnergy < amount)
            return false;

        CurrentEnergy -= amount;
        Notify();
        return true;
    }

    private void Notify()
    {
        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
    }
}
