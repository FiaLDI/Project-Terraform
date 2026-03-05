using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Stats.Adapter;
using Features.UI;

public class EnergyBarUI : PlayerBoundUIView
{
    [Header("UI")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI label;

    [Header("Smooth")]
    [SerializeField] private float smoothSpeed = 10f;

    private EnergyStatsAdapter energy;
    private float targetFill;

    protected override void OnPlayerBound(GameObject player)
    {
        var statsAdapter = player.GetComponent<StatsFacadeAdapter>();
        if (statsAdapter == null)
        {
            Debug.LogError("[EnergyBarUI] StatsFacadeAdapter not found", this);
            return;
        }

        energy = statsAdapter.EnergyStats;
        if (energy == null)
        {
            Debug.LogError("[EnergyBarUI] EnergyStatsAdapter not found", this);
            return;
        }

        energy.OnEnergyChanged += UpdateView;

        // если снапшот уже был
        if (energy.IsReady)
            UpdateView(energy.Current, energy.Max);
    }

    protected override void OnPlayerUnbound(GameObject player)
    {
        if (energy != null)
            energy.OnEnergyChanged -= UpdateView;

        energy = null;
        targetFill = 0f;
    }

    private void UpdateView(float current, float max)
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
