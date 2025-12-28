using UnityEngine;
using System;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Stats.Domain;
using FishNet.Object;


public class BuffExecutor : NetworkBehaviour
{
    private readonly Dictionary<BuffStat, Action<BuffInstance, bool>> handlers = new();

    public static BuffExecutor Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            RegisterHandlers();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void Apply(BuffInstance inst)
    {
        // 🟢 ИСПРАВЛЕНИЕ: Проверяем все нужные поля перед использованием
        if (!IsValid(inst))
        {
            Debug.LogWarning($"[BuffExecutor] Invalid buff instance: {inst?.Config?.buffId}", this);
            return;
        }

        // 🟢 ИСПРАВЛЕНИЕ: Проверяем что handler существует
        if (!handlers.TryGetValue(inst.Config.stat, out var h))
        {
            Debug.LogWarning($"[BuffExecutor] No handler for stat: {inst.Config.stat}", this);
            return;
        }

        Debug.Log($"[BuffExecutor] Applying buff: {inst.Config.buffId} to {inst.Target}", this);
        
        try
        {
            h?.Invoke(inst, true);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BuffExecutor] Error applying buff: {ex.Message}", this);
        }

        // 🟢 ИСПРАВЛЕНИЕ: RPC только если это сервер
        if (IsServer)
        {
            RpcApplyBuff(inst.Config.buffId, inst.Target.GameObject.name);
        }
    }

    public void Expire(BuffInstance inst)
    {
        // 🟢 ИСПРАВЛЕНИЕ: Проверяем что instance валиден
        if (!IsValid(inst))
        {
            Debug.LogWarning($"[BuffExecutor] Cannot expire invalid buff", this);
            return;
        }

        // 🟢 ИСПРАВЛЕНИЕ: Проверяем что handler существует
        if (!handlers.TryGetValue(inst.Config.stat, out var h))
        {
            Debug.LogWarning($"[BuffExecutor] No handler for stat: {inst.Config.stat}", this);
            return;
        }

        Debug.Log($"[BuffExecutor] Expiring buff: {inst.Config.buffId}", this);
        
        try
        {
            h?.Invoke(inst, false);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BuffExecutor] Error expiring buff: {ex.Message}", this);
        }

        // 🟢 ИСПРАВЛЕНИЕ: RPC только если это сервер
        if (IsServer)
        {
            RpcExpireBuff(inst.Config.buffId, inst.Target.GameObject.name);
        }
    }

    public void Tick(BuffInstance inst, float dt)
    {
        if (!IsValid(inst))
            return;

        if (inst.Config.stat == BuffStat.HealPerSecond)
        {
            // 🟢 ИСПРАВЛЕНИЕ: Проверяем что Health не null
            if (inst.Target.Stats?.Health != null)
            {
                inst.Target.Stats.Health.Heal(inst.Config.value * dt);
            }
        }
    }

    private bool IsValid(BuffInstance inst)
    {
        // 🟢 ИСПРАВЛЕНИЕ: Все проверки перед использованием
        if (inst == null)
        {
            return false;
        }

        if (inst.Config == null)
        {
            Debug.LogWarning($"[BuffExecutor] BuffInstance has null Config", this);
            return false;
        }

        if (inst.Target == null)
        {
            Debug.LogWarning($"[BuffExecutor] BuffInstance has null Target", this);
            return false;
        }

        if (inst.Target.Stats == null)
        {
            Debug.LogWarning($"[BuffExecutor] Target {inst.Target} has no Stats", this);
            return false;
        }

        if (inst.Target.GameObject == null)
        {
            Debug.LogWarning($"[BuffExecutor] Target {inst.Target} has no GameObject", this);
            return false;
        }

        return true;
    }

    /* ================= RPC ================= */

    [ObserversRpc]
    private void RpcApplyBuff(string buffId, string targetName)
    {
        Debug.Log($"[BuffExecutor] RPC: Applying buff {buffId} to {targetName}", this);
        
        if (IsServer)
            return; // Сервер уже применил локально

        // На клиенте находим игрока и применяем баф
        var target = FindTargetByName(targetName);
        if (target?.GameObject != null)
        {
            var buffSystem = target.GameObject.GetComponent<BuffSystem>();
            if (buffSystem != null && buffSystem.ServiceReady)
            {
                var buffSO = FindBuffById(buffId);
                if (buffSO != null)
                {
                    buffSystem.Add(buffSO);
                }
            }
        }
    }

    [ObserversRpc]
    private void RpcExpireBuff(string buffId, string targetName)
    {
        Debug.Log($"[BuffExecutor] RPC: Expiring buff {buffId} from {targetName}", this);

        var target = FindTargetByName(targetName);
        if (target?.GameObject != null)
        {
            var buffSystem = target.GameObject.GetComponent<BuffSystem>();
            // 🟢 ИСПРАВЛЕНИЕ: Проверяем что Service и Active не null
            if (buffSystem?.ServiceReady == true && buffSystem.Service?.Active != null)
            {
                var toRemove = new List<BuffInstance>();
                foreach (var buff in buffSystem.Service.Active)
                {
                    if (buff.Config.buffId == buffId)
                        toRemove.Add(buff);
                }

                foreach (var buff in toRemove)
                {
                    buffSystem.Remove(buff);
                }
            }
        }
    }

    /* ================= HELPERS ================= */

    private IBuffTarget FindTargetByName(string targetName)
    {
        var obj = GameObject.Find(targetName);
        if (obj != null)
        {
            return obj.GetComponent<IBuffTarget>();
        }
        
        Debug.LogWarning($"[BuffExecutor] Target not found: {targetName}", this);
        return null;
    }

    private BuffSO FindBuffById(string buffId)
    {
        var allBuffs = Resources.LoadAll<BuffSO>("Buffs");
        foreach (var buff in allBuffs)
        {
            if (buff.buffId == buffId)
                return buff;
        }
        
        Debug.LogWarning($"[BuffExecutor] Buff not found: {buffId}", this);
        return null;
    }

    private void RegisterHandlers()
    {
        // PLAYER
        Register(BuffStat.PlayerDamage,
            (inst, apply) => inst.Target.Stats.Combat.ApplyBuff(inst.Config, apply));

        Register(BuffStat.PlayerMoveSpeed,
            (inst, apply) => inst.Target.Stats.Movement.ApplyBuff(inst.Config, apply));

        Register(BuffStat.PlayerMoveSpeedMult,
            (inst, apply) => inst.Target.Stats.Movement.ApplyBuff(inst.Config, apply));

        Register(BuffStat.PlayerWalkSpeed,
            (inst, apply) => inst.Target.Stats.Movement.ApplyBuff(inst.Config, apply));

        Register(BuffStat.PlayerWalkSpeedMult,
            (inst, apply) => inst.Target.Stats.Movement.ApplyBuff(inst.Config, apply));

        Register(BuffStat.PlayerSprintSpeed,
            (inst, apply) => inst.Target.Stats.Movement.ApplyBuff(inst.Config, apply));

        Register(BuffStat.PlayerSprintSpeedMult,
            (inst, apply) => inst.Target.Stats.Movement.ApplyBuff(inst.Config, apply));

        Register(BuffStat.PlayerCrouchSpeed,
            (inst, apply) => inst.Target.Stats.Movement.ApplyBuff(inst.Config, apply));

        Register(BuffStat.PlayerCrouchSpeedMult,
            (inst, apply) => inst.Target.Stats.Movement.ApplyBuff(inst.Config, apply));

        Register(BuffStat.PlayerShield,
            (inst, apply) => inst.Target.Stats.Health.ApplyBuff(inst.Config, apply));

        Register(BuffStat.PlayerHp,
            (inst, apply) => inst.Target.Stats.Health.ApplyBuff(inst.Config, apply));

        Register(BuffStat.PlayerHpRegen,
            (inst, apply) => inst.Target.Stats.Health.ApplyBuff(inst.Config, apply));

        Register(BuffStat.PlayerEnergyRegen,
            (inst, apply) => inst.Target.Stats.Energy.ApplyBuff(inst.Config, apply));

        Register(BuffStat.PlayerMaxEnergy,
            (inst, apply) => inst.Target.Stats.Energy.ApplyBuff(inst.Config, apply));

        Register(BuffStat.PlayerEnergyCostReduction,
            (inst, apply) => inst.Target.Stats.Energy.ApplyBuff(inst.Config, apply));

        Register(BuffStat.PlayerMiningSpeed,
            (inst, apply) => inst.Target.Stats.Mining.ApplyBuff(inst.Config, apply));
        
        Register(BuffStat.RotationSpeed,
            (inst, apply) => inst.Target.Stats.Movement.ApplyBuff(inst.Config, apply));

        Register(BuffStat.RotationSpeedMult,
            (inst, apply) => inst.Target.Stats.Movement.ApplyBuff(inst.Config, apply));

        // TURRET
        Register(BuffStat.TurretDamage,
            (inst, apply) => inst.Target.Stats.Combat.ApplyBuff(inst.Config, apply));

        Register(BuffStat.TurretRotationSpeed,
            (inst, apply) => inst.Target.Stats.Movement.ApplyBuff(inst.Config, apply));

        Register(BuffStat.TurretMaxHP,
            (inst, apply) => inst.Target.Stats.Health.ApplyBuff(inst.Config, apply));

        Register(BuffStat.TurretFireRate,
            (inst, apply) =>
            {
                if (inst.Target.Stats.Combat is ITurretCombatStats tc)
                    tc.ApplyFireRateBuff(inst.Config, apply);
            });
    }

    private void Register(BuffStat stat, Action<BuffInstance, bool> handler)
    {
        handlers[stat] = handler;
    }
}
