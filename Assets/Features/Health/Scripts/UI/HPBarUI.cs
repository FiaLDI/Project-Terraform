using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Stats.Adapter;

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
        if (adapter != null)
            adapter.OnHealthChanged -= UpdateView;
    }

    public void Bind(HealthStatsAdapter a)
    {
        if (adapter != null)
            adapter.OnHealthChanged -= UpdateView;

        adapter = a;

        if (adapter != null)
        {
            adapter.OnHealthChanged += UpdateView;
            UpdateView(adapter.CurrentHp, adapter.MaxHp);
        }
        else
        {
            Debug.LogWarning("[HPBarUI] Bind() received NULL!");
        }
    }

    private void UpdateView(float current, float max)
    {
        targetFill = max > 0 ? current / max : 0f;

        if (label)
            label.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
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
