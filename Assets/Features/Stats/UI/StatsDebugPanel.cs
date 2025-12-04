using UnityEngine;
using TMPro;
using Features.Stats.Adapter;
using Features.Stats.UnityIntegration;

public class StatsDebugPanel : MonoBehaviour
{
    public TextMeshProUGUI label;

    private StatsFacadeAdapter adapter;

    private void Update()
    {
        // Если адаптера нет — пробуем найти через PlayerRegistry
        if (adapter == null)
            TryFindAdapter();

        if (adapter == null)
        {
            label.text =
                "<color=red>NO PLAYER STATS</color>";
            return;
        }

        var hp      = adapter.HealthStats;
        var energy  = adapter.EnergyStats;
        var combat  = adapter.CombatStats;
        var move    = adapter.MovementStats;
        var mining  = adapter.MiningStats;

        label.text =
$@"=== PLAYER STATS DEBUG ===

<color=#FFD090>HEALTH</color>
HP: {(hp != null ? $"{hp.CurrentHp:0}/{hp.MaxHp:0}" : "NO")}
Regen: {(hp != null ? $"{hp.Regen:0.0}" : "NO")}

<color=#90E0FF>ENERGY</color>
Energy: {(energy != null ? $"{energy.CurrentEnergy:0}/{energy.MaxEnergy:0}" : "NO")}
Regen: {(energy != null ? $"{energy.Regen:0.0}" : "NO")}
Cost Mult: {(energy != null ? $"x{energy.CostMultiplier:0.00}" : "NO")}

<color=#FFB060>COMBAT</color>
Damage Mult: {(combat != null ? $"x{combat.DamageMultiplier:0.00}" : "NO")}

<color=#A0FF90>MOVEMENT</color>
BaseSpeed: {(move != null ? $"{move.BaseSpeed:0.00}" : "NO")}
WalkSpeed: {(move != null ? $"{move.WalkSpeed:0.00}" : "NO")}
SprintSpeed: {(move != null ? $"{move.SprintSpeed:0.00}" : "NO")}
CrouchSpeed: {(move != null ? $"{move.CrouchSpeed:0.00}" : "NO")}

<color=#E0FF80>MINING</color>
Power: {(mining != null ? $"{mining.MiningPower:0.00}" : "NO")}
";
    }

    private void TryFindAdapter()
    {
        // Через PlayerRegistry (лучший способ)
        if (PlayerRegistry.Instance != null)
        {
            adapter = PlayerRegistry.Instance.LocalAdapter;
            if (adapter != null) return;
        }

        // Фоллбэк — поиск PlayerStats на сцене
        var player = FindAnyObjectByType<PlayerStats>();
        if (player != null)
            adapter = player.GetFacadeAdapter();
    }
}
