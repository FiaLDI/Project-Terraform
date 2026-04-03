using UnityEngine;
using TMPro;
using System.Text;
using Features.UI;
using Features.Stats.Adapter;

public class StatsAdvancedDebugUI : PlayerBoundUIView
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI text2;

    private StatsFacadeAdapter stats;

    // ================= SERVER MODE =================
    private bool useServerData;
    private StatsDebugData serverData;

    private readonly StringBuilder sb = new StringBuilder(1024);
    
    private readonly StringBuilder sb2 = new StringBuilder(1024);

    protected override void OnPlayerBound(GameObject player)
    {
        stats = player.GetComponent<StatsFacadeAdapter>();
    }

    protected override void OnPlayerUnbound(GameObject player)
    {
        stats = null;
        useServerData = false;
    }

    private void Update()
    {
        if (text == null)
            return;

        sb.Clear();

        DrawCombat();
        DrawHP();
        DrawEnergy();
        DrawMovement();

        sb2.Clear();
        DrawResist();
        text2.text = sb2.ToString();

        text.text = sb.ToString();
    }

    // =====================================================
    // SERVER CONTROL
    // =====================================================

    public void ApplyServerData(StatsDebugData data)
    {
        serverData = data;
        useServerData = true;
    }

    public void DisableServerMode()
    {
        useServerData = false;
    }

    // =====================================================
    // SECTIONS
    // =====================================================

    private void DrawCombat()
    {
        sb.AppendLine("=== COMBAT ===");

        if (useServerData)
        {
            sb.AppendLine($"Damage: {serverData.damage:0.##}");
            sb.AppendLine($"FireRate: {serverData.fireRate:0.##}");
            sb.AppendLine($"Spread: {serverData.spread:0.##}");
            sb.AppendLine($"AimSpread: {serverData.aimSpread:0.##}");
            sb.AppendLine($"Range: {serverData.range:0.##}");
            sb.AppendLine($"Recoil: {serverData.recoil:0.##}");
            sb.AppendLine($"Magazine: {serverData.magazine}");

            sb.AppendLine($"CritChance: {serverData.critChance * 100f:0.#}%");
            sb.AppendLine($"CritMult: {serverData.critMultiplier:0.##}");
            sb.AppendLine($"Penetration: {serverData.penetration:0.##}");
        }
        else
        {
            if (stats == null) return;

            var c = stats.CombatStats;
            if (c == null) return;

            sb.AppendLine($"Damage: {c.FinalDamage:0.##}");
            sb.AppendLine($"FireRate: {c.FireRate:0.##}");
            sb.AppendLine($"Spread: {c.Spread:0.##}");
            sb.AppendLine($"AimSpread: {c.AimSpread:0.##}");
            sb.AppendLine($"Range: {c.Range:0.##}");
            sb.AppendLine($"Recoil: {c.Recoil:0.##}");
            sb.AppendLine($"Magazine: {c.MagazineSize}");

            sb.AppendLine($"CritChance: {c.CritChance * 100f:0.#}%");
            sb.AppendLine($"CritMult: {c.CritMultiplier:0.##}");
            sb.AppendLine($"Penetration: {c.Penetration:0.##}");
        }

        sb.AppendLine();
    }

    private void DrawHP()
    {
        sb.AppendLine("=== HEALTH ===");

        if (useServerData)
        {
            sb.AppendLine($"HP: {serverData.hp:0} / {serverData.maxHp:0}");
            sb.AppendLine($"Shield: {serverData.shield:0} / {serverData.maxShield:0}");
        }
        else
        {
            if (stats == null) return;

            var h = stats.HealthStats;
            if (h == null) return;

            sb.AppendLine($"HP: {h.CurrentHp:0} / {h.MaxHp:0}");
            sb.AppendLine($"Shield: {h.CurrentShield:0} / {h.MaxShield:0}");
        }

        sb.AppendLine();
    }

    private void DrawEnergy()
    {
        sb.AppendLine("=== ENERGY ===");

        if (useServerData)
        {
            sb.AppendLine($"Energy: {serverData.energy:0} / {serverData.maxEnergy:0}");
            sb.AppendLine($"Regen: {serverData.regen:0.##}");
            sb.AppendLine($"CostMult: {serverData.costMult:0.##}");
        }
        else
        {
            if (stats == null) return;

            var e = stats.EnergyStats;
            if (e == null) return;

            sb.AppendLine($"Energy: {e.Current:0} / {e.Max:0}");
            sb.AppendLine($"Regen: {e.Regen:0.##}");
            sb.AppendLine($"CostMult: {e.CostMultiplier:0.##}");
        }

        sb.AppendLine();
    }

    private void DrawMovement()
    {
        sb.AppendLine("=== MOVEMENT ===");

        if (useServerData)
        {
            sb.AppendLine($"Walk: {serverData.walk:0.##}");
            sb.AppendLine($"Sprint: {serverData.sprint:0.##}");
            sb.AppendLine($"Crouch: {serverData.crouch:0.##}");
            sb.AppendLine($"Rotation: {serverData.rotation:0.##}");
            sb.AppendLine($"Gravity: {serverData.gravity:0.##}");
            sb.AppendLine($"Jump: {serverData.jump:0.##}");
        }
        else
        {
            if (stats == null) return;

            var m = stats.MovementStats;
            if (m == null) return;

            sb.AppendLine($"Walk: {m.WalkSpeed:0.##}");
            sb.AppendLine($"Sprint: {m.SprintSpeed:0.##}");
            sb.AppendLine($"Crouch: {m.CrouchSpeed:0.##}");
            sb.AppendLine($"Rotation: {m.RotationSpeed:0.##}");
            sb.AppendLine($"Gravity: {m.Gravity:0.##}");
            sb.AppendLine($"Jump: {m.JumpHeight:0.##}");
        }

        sb.AppendLine();
    }

    private void DrawResist()
    {
        sb2.AppendLine("=== RESIST ===");

        if (useServerData)
        {
            sb2.AppendLine($"Generic: {FormatPercent(serverData.generic)}");
            sb2.AppendLine($"Explosion: {FormatPercent(serverData.explosion)}");
            sb2.AppendLine($"Energy: {FormatPercent(serverData.energyRes)}");
            sb2.AppendLine($"Mining: {FormatPercent(serverData.mining)}");
            sb2.AppendLine($"Melee: {FormatPercent(serverData.melee)}");
            sb2.AppendLine($"Fire: {FormatPercent(serverData.fire)}");
            sb2.AppendLine($"Electric: {FormatPercent(serverData.electric)}");
            sb2.AppendLine($"Poison: {FormatPercent(serverData.poison)}");
            sb2.AppendLine($"Frost: {FormatPercent(serverData.frost)}");
            sb2.AppendLine($"Acid: {FormatPercent(serverData.acid)}");
        }
        else
        {
            if (stats == null) return;

            var p = stats.ProtectStats;
            if (p == null) return;

            sb2.AppendLine($"Generic: {FormatPercent(p.GenericResistance)}");
            sb2.AppendLine($"Explosion: {FormatPercent(p.ExplosionResistance)}");
            sb2.AppendLine($"Energy: {FormatPercent(p.EnergyResistance)}");
            sb2.AppendLine($"Mining: {FormatPercent(p.MiningResistance)}");
            sb2.AppendLine($"Melee: {FormatPercent(p.MeleeResistance)}");
            sb2.AppendLine($"Fire: {FormatPercent(p.FireResistance)}");
            sb2.AppendLine($"Electric: {FormatPercent(p.ElectricResistance)}");
            sb2.AppendLine($"Poison: {FormatPercent(p.PoisonResistance)}");
            sb2.AppendLine($"Frost: {FormatPercent(p.FrostResistance)}");
            sb2.AppendLine($"Acid: {FormatPercent(p.AcidResistance)}");
        }

        sb2.AppendLine();
    }

    // =====================================================
    // HELPERS
    // =====================================================

    private string FormatPercent(float v)
    {
        return $"{v * 100f:0.#}%";
    }
}
