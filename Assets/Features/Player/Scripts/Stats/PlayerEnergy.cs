using UnityEngine;
using System;

public class PlayerEnergy : MonoBehaviour
{
    [Header("Base energy settings")]
    [SerializeField] private float baseMaxEnergy = 100f;
    [SerializeField] private float baseRegenPerSecond = 8f;

    public float CurrentEnergy { get; private set; }

    private BuffSystem buffSystem;

    private float costReductionPercent = 0f;

    private bool initialized = false;

    private void Awake()
    {
        buffSystem = GetComponent<BuffSystem>();

        if (buffSystem == null)
        {
            enabled = false;
            return;
        }

        CurrentEnergy = baseMaxEnergy;
        initialized = true;
    }

    private void Start()
    {
        if (!initialized) return;
        Notify();
    }

    private void Update()
    {
        if (!initialized) return;

        float max = MaxEnergy;
        if (CurrentEnergy < max)
        {
            CurrentEnergy += Regen * Time.deltaTime;
            CurrentEnergy = Mathf.Clamp(CurrentEnergy, 0, max);
            Notify();
        }
    }

    // ============================================================
    // MAX ENERGY
    // ============================================================
    public float MaxEnergy
    {
        get
        {
            float bonus = 0f;

            if (buffSystem != null)
            {
                foreach (var b in buffSystem.Active)
                    if (b.Config is MaxEnergyBuffSO maxBuff)
                        bonus += maxBuff.extraMaxEnergy;
            }

            // глобальные бафы
            if (GlobalBuffSystem.I != null)
                bonus += GlobalBuffSystem.I.GetValue("player_max_energy");

            return baseMaxEnergy + bonus;
        }
    }

    // ============================================================
    // REGEN
    // ============================================================
    public float Regen
    {
        get
        {
            float bonus = 0f;

            if (buffSystem != null)
            {
                foreach (var b in buffSystem.Active)
                    if (b.Config is EnergyRegenBuffSO regenBuff)
                        bonus += regenBuff.bonusRegen;
            }

            if (GlobalBuffSystem.I != null)
                bonus += GlobalBuffSystem.I.GetValue("player_regen");

            return baseRegenPerSecond + bonus;
        }
    }

    // ============================================================
    // COST REDUCTION (abilities)
    // ============================================================
    public void AddCostReduction(float amount)
    {
        costReductionPercent += amount;
    }

    public void RemoveCostReduction(float amount)
    {
        costReductionPercent -= amount;
        if (costReductionPercent < 0f)
            costReductionPercent = 0f;
    }

    public float GetActualCost(float cost)
    {
        float mult = (1f - costReductionPercent / 100f);
        if (mult < 0.05f) mult = 0.05f;
        return cost * mult;
    }

    // ============================================================
    // DIRECT CONTROL API
    // ============================================================
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
        Notify();
    }

    public bool HasEnergy(float amount)
    {
        return CurrentEnergy >= amount;
    }

    public bool TrySpend(float amount)
    {
        float actual = GetActualCost(amount);

        if (CurrentEnergy < actual)
            return false;

        CurrentEnergy -= actual;
        Notify();
        return true;
    }

    private void Notify()
    {
        OnEnergyChanged?.Invoke(CurrentEnergy, MaxEnergy);
    }

    public event Action<float, float> OnEnergyChanged;
}
