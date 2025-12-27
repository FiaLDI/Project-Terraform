using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Stats.Adapter;
using Features.Stats.UnityIntegration;
using Features.UI;

public class HPBarUI : PlayerBoundUIView
{
    [Header("UI")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI label;

    [Header("Smooth")]
    [SerializeField] private float smoothSpeed = 10f;

    private HealthStatsAdapter health;
    private float targetFill;
    private PlayerStats boundStats;

    // =====================================================
    // PLAYER BIND (FROM BASE)
    // =====================================================

    protected override void OnPlayerBound(GameObject player)
    {
        Debug.Log("[HPBarUI] OnPlayerBound called", this);

        boundStats = player.GetComponent<PlayerStats>();
        if (boundStats == null)
        {
            Debug.LogError("[HPBarUI] PlayerStats not found on player", this);
            return;
        }

        // ÊËÞ×ÅÂÎÉ ÔÈÕ: Ïðîâåðÿåì ãîòîâíîñòü àäàïòåðà
        var adapter = boundStats.Adapter;
        if (adapter == null)
        {
            Debug.LogWarning("[HPBarUI] Adapter is null, waiting for ready event", this);
            PlayerStats.OnStatsReady += HandleStatsReady;
            return;
        }

        if (!adapter.IsReady)
        {
            Debug.LogWarning("[HPBarUI] Adapter not ready yet, waiting...", this);
            PlayerStats.OnStatsReady += HandleStatsReady;
            return;
        }

        Debug.Log("[HPBarUI] Adapter ready, binding health stats", this);
        Bind(adapter.HealthStats);
    }

    protected override void OnPlayerUnbound(GameObject player)
    {
        Debug.Log("[HPBarUI] OnPlayerUnbound called", this);
        PlayerStats.OnStatsReady -= HandleStatsReady;
        Unbind();
    }

    // =====================================================
    // STATS READY
    // =====================================================

    private void HandleStatsReady(PlayerStats stats)
    {
        Debug.Log("[HPBarUI] HandleStatsReady called", this);

        if (stats != boundStats)
        {
            Debug.Log("[HPBarUI] Stats mismatch, ignoring", this);
            return;
        }

        PlayerStats.OnStatsReady -= HandleStatsReady;

        if (stats.Adapter != null && stats.Adapter.IsReady)
        {
            Debug.Log("[HPBarUI] Now binding health stats", this);
            Bind(stats.Adapter.HealthStats);
        }
    }

    // =====================================================
    // BIND / UNBIND
    // =====================================================

    private void Bind(HealthStatsAdapter adapter)
    {
        if (adapter == null)
        {
            Debug.LogError("[HPBarUI] Cannot bind null adapter", this);
            return;
        }

        Debug.Log("[HPBarUI] Successfully bound to health adapter", this);

        health = adapter;
        health.OnHealthChanged += UpdateHp;
        UpdateHp(health.CurrentHp, health.MaxHp);
    }

    private void Unbind()
    {
        if (health != null)
            health.OnHealthChanged -= UpdateHp;

        health = null;
        boundStats = null;
        targetFill = 0f;
    }

    // =====================================================
    // VIEW
    // =====================================================

    private void UpdateHp(float current, float max)
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