using UnityEngine;
using System;
using Features.Buffs.Application;
using Features.Buffs.UnityIntegration;
using Features.Buffs.Domain;

public class PlayerEnergy : MonoBehaviour
{
    [Header("Base energy settings")]
    [SerializeField] private float baseMaxEnergy = 100f;
    [SerializeField] private float baseRegenPerSecond = 8f;

    public float CurrentEnergy { get; private set; }

    private BuffSystem buffSystem;

    private float costReductionPercent = 0f;
    private float regenBonusPercent = 0f;

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
    // MAX ENERGY (UNIVERSAL BUFFS)
    // ============================================================
    public float MaxEnergy
    {
        get
        {
            float bonus = 0f;

            // --- Безопасная проверка BuffSystem ---
            if (buffSystem != null && buffSystem.Active != null)
            {
                foreach (var inst in buffSystem.Active)
                {
                    if (inst?.Config == null)
                        continue;

                    if (inst.Config.stat == BuffStat.PlayerMaxEnergy)
                    {
                        switch (inst.Config.modType)
                        {
                            case BuffModType.Add:
                                bonus += inst.Config.value;
                                break;

                            case BuffModType.Mult:
                                bonus += baseMaxEnergy * (inst.Config.value - 1f);
                                break;

                            case BuffModType.Set:
                                return inst.Config.value;
                        }
                    }
                }
            }

            // --- Безопасная проверка GlobalBuffSystem ---
            if (GlobalBuffSystem.I != null)
            {
                try
                {
                    bonus += GlobalBuffSystem.I.GetValue("player_max_energy");
                }
                catch { /* просто пропускаем */ }
            }

            return baseMaxEnergy + bonus;
        }
    }



    // ============================================================
    // REGEN (UNIVERSAL BUFFS)
    // ============================================================
    public float Regen
    {
        get
        {
            float bonusFlat = 0f;

            if (buffSystem != null && buffSystem.Active != null)
            {
                foreach (var inst in buffSystem.Active)
                {
                    if (inst?.Config == null)
                        continue;

                    if (inst.Config.stat == BuffStat.PlayerEnergyRegen)
                    {
                        switch (inst.Config.modType)
                        {
                            case BuffModType.Add:
                                bonusFlat += inst.Config.value;
                                break;

                            case BuffModType.Mult:
                                bonusFlat += baseRegenPerSecond * (inst.Config.value - 1f);
                                break;

                            case BuffModType.Set:
                                return inst.Config.value;
                        }
                    }
                }
            }

            if (GlobalBuffSystem.I != null)
            {
                try
                {
                    bonusFlat += GlobalBuffSystem.I.GetValue("player_regen");
                }
                catch {}
            }

            float scaledBase = baseRegenPerSecond * (1f + regenBonusPercent / 100f);
            return scaledBase + bonusFlat;
        }
    }


    public void AddRegenPercent(float value)
    {
        regenBonusPercent += value;
    }

    public void RemoveRegenPercent(float value)
    {
        regenBonusPercent -= value;
        if (regenBonusPercent < 0f)
            regenBonusPercent = 0f;
    }

    // ============================================================
    // COST REDUCTION
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
    // API
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
