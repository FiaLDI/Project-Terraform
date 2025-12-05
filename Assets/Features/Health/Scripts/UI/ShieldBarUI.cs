using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Stats.Adapter;

public class ShieldBarUI : MonoBehaviour
{
    [Header("UI")]
    public Image fillImage;
    public TextMeshProUGUI label;

    [Header("Smooth")]
    public float smoothSpeed = 10f;

    private HealthStatsAdapter adapter;
    private float targetFill = 0f;

    private void Start()
    {
        adapter = GetComponentInParent<HealthStatsAdapter>();

        if (adapter == null)
        {
            Debug.LogWarning("ShieldBarUI: No HealthStatsAdapter found!", this);
            return;
        }

        adapter.OnShieldChanged += UpdateView;
        UpdateView(adapter.CurrentShield, adapter.MaxShield);
    }

    private void OnDestroy()
    {
        if (adapter != null)
            adapter.OnShieldChanged -= UpdateView;
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
