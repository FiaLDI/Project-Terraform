using UnityEngine;
using System;

public class PlayerEnergy : MonoBehaviour
{
    [Header("Base energy settings")]
    [SerializeField] private float baseMaxEnergy = 100f;
    [SerializeField] private float baseRegenPerSecond = 8f;

    public float CurrentEnergy { get; private set; }
    public float MaxEnergy => baseMaxEnergy + (buffSystem?.GetTotal(BuffType.MaxEnergy) ?? 0f);
    public float Regen => baseRegenPerSecond + (buffSystem?.GetTotal(BuffType.EnergyRegen) ?? 0f);

    public event Action<float, float> OnEnergyChanged;

    private BuffSystem buffSystem;

    private void Awake()
    {
        buffSystem = GetComponent<BuffSystem>();
    }

    private void Start()
    {
        CurrentEnergy = MaxEnergy;
        Notify();
    }

    private void Update()
    {
        // реген энергии с учётом баффов
        if (CurrentEnergy < MaxEnergy)
        {
            CurrentEnergy += Regen * Time.deltaTime;
            CurrentEnergy = Mathf.Clamp(CurrentEnergy, 0, MaxEnergy);
            Notify();
        }
    }

    // ==========================================
    // BASE SETTINGS
    // ==========================================

    public void SetMaxEnergy(float value, bool fill)
    {
        baseMaxEnergy = value;

        if (fill)
            CurrentEnergy = MaxEnergy;

        Notify();
    }

    public void SetRegen(float value)
    {
        baseRegenPerSecond = value;
    }

    // ==========================================
    // ENERGY SPENDING
    // ==========================================

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

    // ==========================================
    private void Notify()
    {
        OnEnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);
    }
}
