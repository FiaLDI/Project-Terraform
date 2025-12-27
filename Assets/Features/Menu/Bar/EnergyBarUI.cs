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
        Debug.Log("[EnergyBarUI] OnPlayerBound called", this);

        boundStats = player.GetComponent<PlayerStats>();
        if (boundStats == null)
        {
            Debug.LogError("[EnergyBarUI] PlayerStats not found on player", this);
            return;
        }

        // ÊËÞ×ÅÂÎÉ ÔÈÕ: Ïðîâåðÿåì ãîòîâíîñòü àäàïòåðà
        var adapter = boundStats.Adapter;
        if (adapter == null)
        {
            Debug.LogWarning("[EnergyBarUI] Adapter is null, waiting for ready event", this);
            PlayerStats.OnStatsReady += HandleStatsReady;
            return;
        }

        if (!adapter.IsReady)
        {
            Debug.LogWarning("[EnergyBarUI] Adapter not ready yet, waiting...", this);
            PlayerStats.OnStatsReady += HandleStatsReady;
            return;
        }

        Debug.Log("[EnergyBarUI] Adapter ready, binding energy stats", this);
        Bind(adapter.EnergyStats);
    }

    protected override void OnPlayerUnbound(GameObject player)
    {
        Debug.Log("[EnergyBarUI] OnPlayerUnbound called", this);
        PlayerStats.OnStatsReady -= HandleStatsReady;
        Unbind();
    }

    // =====================================================
    // STATS READY
    // =====================================================

    private void HandleStatsReady(PlayerStats stats)
    {
        Debug.Log("[EnergyBarUI] HandleStatsReady called", this);

        if (stats != boundStats)
        {
            Debug.Log("[EnergyBarUI] Stats mismatch, ignoring", this);
            return;
        }

        PlayerStats.OnStatsReady -= HandleStatsReady;

        if (stats.Adapter != null && stats.Adapter.IsReady)
        {
            Debug.Log("[EnergyBarUI] Now binding energy stats", this);
            Bind(stats.Adapter.EnergyStats);
        }
    }

    // =====================================================
    // BIND / UNBIND
    // =====================================================

    private void Bind(IEnergyView view)
    {
        if (view == null)
        {
            Debug.LogError("[EnergyBarUI] Cannot bind null view", this);
            return;
        }

        Debug.Log("[EnergyBarUI] Successfully bound to energy view", this);

        energy = view;
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