using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Stats.Domain;
using Features.Stats.UnityIntegration;
using Features.UI;

public class EnergyBarUI : PlayerBoundUIView
{
    [Header("UI")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI label;

    [Header("Smooth")]
    [SerializeField] private float smoothSpeed = 10f;

    private IEnergyView energy;
    private float targetFill;
    private PlayerStats boundStats;

    // =====================================================
    // PLAYER BIND (FROM BASE)
    // =====================================================

    protected override void OnPlayerBound(GameObject player)
    {
        boundStats = player.GetComponent<PlayerStats>();
        if (boundStats == null)
            return;

        var adapter = boundStats.Adapter;
        if (adapter == null)
            return;

        if (!adapter.IsReady)
        {
            PlayerStats.OnStatsReady += HandleStatsReady;
            return;
        }

        Bind(adapter.EnergyStats);
    }

    protected override void OnPlayerUnbound(GameObject player)
    {
        PlayerStats.OnStatsReady -= HandleStatsReady;
        Unbind();
    }

    // =====================================================
    // STATS READY
    // =====================================================

    private void HandleStatsReady(PlayerStats stats)
    {
        if (stats != boundStats)
            return;

        PlayerStats.OnStatsReady -= HandleStatsReady;

        if (stats.Adapter != null)
            Bind(stats.Adapter.EnergyStats);
    }

    // =====================================================
    // BIND / UNBIND
    // =====================================================

    private void Bind(IEnergyView view)
    {
        energy = view;
        if (energy == null)
            return;

        energy.OnEnergyChanged += UpdateView;
        UpdateView(energy.CurrentEnergy, energy.MaxEnergy);
    }

    private void Unbind()
    {
        if (energy != null)
            energy.OnEnergyChanged -= UpdateView;

        energy = null;
        boundStats = null;
        targetFill = 0f;
    }

    // =====================================================
    // VIEW
    // =====================================================

    private void UpdateView(float current, float max)
    {
        targetFill = max > 0 ? current / max : 0f;

        if (label != null)
            label.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
    }

    private void Update()
    {
        if (fillImage == null)
            return;

        fillImage.fillAmount = Mathf.Lerp(
            fillImage.fillAmount,
            targetFill,
            Time.deltaTime * smoothSpeed
        );
    }
}
