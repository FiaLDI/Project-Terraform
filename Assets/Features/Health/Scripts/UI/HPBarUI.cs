using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Stats.Adapter;
using Features.Stats.UnityIntegration;
public class HPBarUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI label;

    [Header("Smooth")]
    [SerializeField] private float smoothSpeed = 10f;

    private HealthStatsAdapter adapter;
    private float targetFill = 1f;

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Start()
    {
        PlayerStats.OnStatsReady += HandleStatsReady;
    }

    private void HandleStatsReady(PlayerStats stats)
    {
        Bind(stats.GetFacadeAdapter().HealthStats);
    }

    private void Unsubscribe()
    {
        if (adapter == null) return;

        adapter.OnHealthChanged -= UpdateHp;
        adapter.OnShieldChanged -= UpdateShield;
    }

    /// <summary>
    /// Привязка к адаптеру HP. Вызывать из PlayerController.HandleStatsReady()
    /// </summary>
    public void Bind(HealthStatsAdapter a)
    {
        Unsubscribe();

        adapter = a;

        if (adapter != null)
        {
            adapter.OnHealthChanged += UpdateHp;
            adapter.OnShieldChanged += UpdateShield;

            // Инициализация UI текущими значениями
            UpdateHp(adapter.CurrentHp, adapter.MaxHp);
        }
        else
        {
            Debug.LogWarning("[HPBarUI] Bind() received NULL!");
        }
    }

    private void UpdateHp(float current, float max)
    {
        targetFill = max > 0 ? current / max : 0f;

        if (label)
            label.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
    }

    private void UpdateShield(float current, float max)
    {
        // OPTIONAL: если хочешь отображение щита в баре
        // пока игнорируем
    }

    private void Update()
    {
        if (!fillImage) return;

        fillImage.fillAmount = Mathf.Lerp(
            fillImage.fillAmount,
            targetFill,
            Time.deltaTime * smoothSpeed
        );
    }
}
