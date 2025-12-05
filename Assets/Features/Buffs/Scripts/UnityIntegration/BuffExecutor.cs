using UnityEngine;
using System;
using System.Collections.Generic;
using Features.Buffs.Domain;
using Features.Buffs.Application;
using Features.Stats.Domain;

public class BuffExecutor : MonoBehaviour
{
    private readonly Dictionary<BuffStat, Action<BuffInstance, bool>> handlers =
        new();

    private void Awake()
    {
        RegisterHandlers();
    }

    public void Apply(BuffInstance inst)
    {
        if (!IsValid(inst)) return;

        if (handlers.TryGetValue(inst.Config.stat, out var h))
            h(inst, true);
    }

    public void Expire(BuffInstance inst)
    {
        if (!IsValid(inst)) return;

        if (handlers.TryGetValue(inst.Config.stat, out var h))
            h(inst, false);
    }

    public void Tick(BuffInstance inst, float dt)
    {
        if (!IsValid(inst)) return;

        if (inst.Config.stat == BuffStat.HealPerSecond)
            inst.Target.Stats.Health.Heal(inst.Config.value * dt);
    }

    private bool IsValid(BuffInstance inst)
    {
        return inst != null &&
               inst.Config != null &&
               inst.Target != null &&
               inst.Target.Stats != null;
    }

    // --------------------------
    // REGISTER HANDLERS
    // --------------------------
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
