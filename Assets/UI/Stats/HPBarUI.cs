using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Stats.Adapter;
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

    protected override void OnPlayerBound(GameObject player)
    {
        var statsAdapter = player.GetComponent<StatsFacadeAdapter>();
        if (statsAdapter == null)
        {
            Debug.LogError("[HPBarUI] StatsFacadeAdapter not found", this);
            return;
        }

        health = statsAdapter.HealthStats;
        if (health == null)
        {
            Debug.LogError("[HPBarUI] HealthStatsAdapter not found", this);
            return;
        }

        health.OnHealthChanged += UpdateHp;

        // если снапшот уже приходил — сразу обновим UI
        if (health.IsReady)
            UpdateHp(health.CurrentHp, health.MaxHp);
    }

    protected override void OnPlayerUnbound(GameObject player)
    {
        if (health != null)
            health.OnHealthChanged -= UpdateHp;

        health = null;
        targetFill = 0f;
    }

    private void UpdateHp(float current, float max)
    {
        targetFill = max > 0f ? current / max : 0f;

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
