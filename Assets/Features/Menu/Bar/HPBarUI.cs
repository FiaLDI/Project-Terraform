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

        Bind(adapter.HealthStats);
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
            Bind(stats.Adapter.HealthStats);
    }

    // =====================================================
    // BIND / UNBIND
    // =====================================================

    private void Bind(HealthStatsAdapter adapter)
    {
        if (adapter == null)
            return;

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
