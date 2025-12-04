using UnityEngine;
using TMPro;
using Features.Stats.Adapter;
using Features.Stats.UnityIntegration;

public class StatsDebugPanel : MonoBehaviour
{
    public StatsFacadeAdapter adapter;
    public TextMeshProUGUI label;

    private void Awake()
    {
        TryFindAdapters();
    }

    private void Update()
    {
        // если адаптера нет — пробуем снова
        if (adapter == null)
        {
            TryFindAdapters();
            if (adapter == null)
            {
                label.text =
                    $"HP: <color=red>NO ADAPTER</color>\n" +
                    $"Energy: <color=red>NO ADAPTER</color>\n" +
                    $"DMG: <color=red>NO ADAPTER</color>\n" +
                    $"Movement: <color=red>NO ADAPTER</color>\n" +
                    $"Mining: <color=red>NO ADAPTER</color>\n";
                return;
            }
        }

        var hp = adapter.HealthStats;
        var energy = adapter.EnergyStats;
        var combat = adapter.CombatStats;
        var movement = adapter.MovementStats;
        var mining = adapter.MiningStats;

        label.text =
            $"HP: {(hp != null ? $"{hp.CurrentHp}/{hp.MaxHp}" : "<color=red>NO</color>")}\n" +
            $"Energy: {(energy != null ? $"{energy.CurrentEnergy}/{energy.MaxEnergy}" : "<color=red>NO</color>")}\n" +
            $"DMG: {(combat != null ? combat.DamageMultiplier.ToString("0.00") : "<color=red>NO</color>")}\n" +
            $"Movement: {(movement != null ? movement.BaseSpeed.ToString("0.00") : "<color=red>NO</color>")}\n" +
            $"Mining: {(mining != null ? mining.MiningPower.ToString("0.00") : "<color=red>NO</color>")}\n";
    }

    private void TryFindAdapters()
    {
        if (adapter != null) return;

        var player = FindAnyObjectByType<PlayerStats>();
        if (player != null)
            adapter = player.GetFacadeAdapter();
    }
}
