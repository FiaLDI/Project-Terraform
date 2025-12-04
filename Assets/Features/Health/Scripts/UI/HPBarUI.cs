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

    private void Start()
    {
        // Auto-bind if Bind() was not called
        if (adapter == null)
        {
            adapter = GetComponentInParent<HealthStatsAdapter>();

            if (adapter != null)
            {
                adapter.OnHealthChanged += UpdateView;
                UpdateView(adapter.CurrentHp, adapter.MaxHp);
            }
            else
            {
                Debug.LogWarning("HPBarUI: HealthStatsAdapter not found.", this);
            }
        }
    }

    private void OnDestroy()
    {
        if (adapter != null)
            adapter.OnHealthChanged -= UpdateView;
    }

    /// <summary>
    /// Bind HPBar to a HealthStatsAdapter
    /// </summary>
    public void Bind(HealthStatsAdapter h)
    {
        if (adapter != null)
            adapter.OnHealthChanged -= UpdateView;

        adapter = h;

        if (adapter != null)
        {
            adapter.OnHealthChanged += UpdateView;
            UpdateView(adapter.CurrentHp, adapter.MaxHp);
        }
    }

    private void UpdateView(float current, float max)
    {
        targetFill = max > 0 ? current / max : 0f;

        if (label != null)
            label.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
    }

    private void Update()
    {
        if (fillImage == null) return;

        fillImage.fillAmount = Mathf.Lerp(
            fillImage.fillAmount,
            targetFill,
            Time.deltaTime * smoothSpeed
        );
    }
}
