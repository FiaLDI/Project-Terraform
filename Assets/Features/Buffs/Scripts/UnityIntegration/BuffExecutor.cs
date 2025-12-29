using UnityEngine;
using System;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Buffs.Application;

public class BuffExecutor : MonoBehaviour
{
    private readonly Dictionary<BuffStat, Action<BuffInstance, bool>> handlers = new();

    public static BuffExecutor Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // На всякий случай отцепляем от родителей
            DontDestroyOnLoad(gameObject); // Живем вечно между сценами
            RegisterHandlers();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Применяет стат баффа локально (вызывается из BuffSystem)
    /// </summary>
    public void Apply(BuffInstance inst)
    {
        if (!IsValid(inst)) return;

        if (!handlers.TryGetValue(inst.Config.stat, out var h))
        {
            Debug.LogWarning($"[BuffExecutor] No handler for stat: {inst.Config.stat}");
            return;
        }

        try
        {
            // true = наложить эффект
            h?.Invoke(inst, true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BuffExecutor] Error applying buff: {ex.Message}");
        }
    }

    /// <summary>
    /// Снимает стат баффа локально
    /// </summary>
    public void Expire(BuffInstance inst)
    {
        if (!IsValid(inst)) return;

        if (!handlers.TryGetValue(inst.Config.stat, out var h)) return;

        try
        {
            // false = снять эффект
            h?.Invoke(inst, false);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BuffExecutor] Error expiring buff: {ex.Message}");
        }
    }

    public void Tick(BuffInstance inst, float dt)
    {
        if (!IsValid(inst)) return;

        // Логика тиковых эффектов (HoT / DoT)
        if (inst.Config.stat == BuffStat.HealPerSecond)
        {
            if (inst.Target.Stats?.Health != null)
            {
                inst.Target.Stats.Health.Heal(inst.Config.value * dt);
            }
        }
    }

    private bool IsValid(BuffInstance inst)
    {
        if (inst == null || inst.Config == null) return false;
        if (inst.Target == null || inst.Target.GameObject == null) return false;
        if (inst.Target.Stats == null) return false;
        return true;
    }

    // ==========================================
    // HANDLERS REGISTRATION
    // ==========================================
    private void RegisterHandlers()
    {
        // PLAYER STATS
        Register(BuffStat.PlayerDamage, (i, apply) => i.Target.Stats.Combat.ApplyBuff(i.Config, apply));
        
        Register(BuffStat.PlayerMoveSpeed, (i, apply) => i.Target.Stats.Movement.ApplyBuff(i.Config, apply));
        Register(BuffStat.PlayerMoveSpeedMult, (i, apply) => i.Target.Stats.Movement.ApplyBuff(i.Config, apply));
        Register(BuffStat.PlayerWalkSpeed, (i, apply) => i.Target.Stats.Movement.ApplyBuff(i.Config, apply));
        Register(BuffStat.PlayerWalkSpeedMult, (i, apply) => i.Target.Stats.Movement.ApplyBuff(i.Config, apply));
        Register(BuffStat.PlayerSprintSpeed, (i, apply) => i.Target.Stats.Movement.ApplyBuff(i.Config, apply));
        Register(BuffStat.PlayerSprintSpeedMult, (i, apply) => i.Target.Stats.Movement.ApplyBuff(i.Config, apply));
        Register(BuffStat.PlayerCrouchSpeed, (i, apply) => i.Target.Stats.Movement.ApplyBuff(i.Config, apply));
        Register(BuffStat.PlayerCrouchSpeedMult, (i, apply) => i.Target.Stats.Movement.ApplyBuff(i.Config, apply));
        
        Register(BuffStat.PlayerShield, (i, apply) => i.Target.Stats.Health.ApplyBuff(i.Config, apply));
        Register(BuffStat.PlayerHp, (i, apply) => i.Target.Stats.Health.ApplyBuff(i.Config, apply));
        Register(BuffStat.PlayerHpRegen, (i, apply) => i.Target.Stats.Health.ApplyBuff(i.Config, apply));
        
        Register(BuffStat.PlayerEnergyRegen, (i, apply) => i.Target.Stats.Energy.ApplyBuff(i.Config, apply));
        Register(BuffStat.PlayerMaxEnergy, (i, apply) => i.Target.Stats.Energy.ApplyBuff(i.Config, apply));
        Register(BuffStat.PlayerEnergyCostReduction, (i, apply) => i.Target.Stats.Energy.ApplyBuff(i.Config, apply));
        
        Register(BuffStat.PlayerMiningSpeed, (i, apply) => i.Target.Stats.Mining.ApplyBuff(i.Config, apply));
        
        Register(BuffStat.RotationSpeed, (i, apply) => i.Target.Stats.Movement.ApplyBuff(i.Config, apply));
        Register(BuffStat.RotationSpeedMult, (i, apply) => i.Target.Stats.Movement.ApplyBuff(i.Config, apply));

        // TURRET STATS
        Register(BuffStat.TurretDamage, (i, apply) => i.Target.Stats.Combat.ApplyBuff(i.Config, apply));
        Register(BuffStat.TurretRotationSpeed, (i, apply) => i.Target.Stats.Movement.ApplyBuff(i.Config, apply));
        Register(BuffStat.TurretMaxHP, (i, apply) => i.Target.Stats.Health.ApplyBuff(i.Config, apply));
        Register(BuffStat.TurretFireRate, (i, apply) =>
        {
            if (i.Target.Stats.Combat is Features.Stats.Domain.ITurretCombatStats tc)
                tc.ApplyFireRateBuff(i.Config, apply);
        });
    }

    private void Register(BuffStat stat, Action<BuffInstance, bool> handler)
    {
        handlers[stat] = handler;
    }
}
