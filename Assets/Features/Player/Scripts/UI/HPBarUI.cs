using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HPBarUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;      // hp_fill (Filled)
    [SerializeField] private Image frameImage;     // hp_frame (Sliced, опционально)
    [SerializeField] private Image iconImage;      // heart icon (опционально)
    [SerializeField] private TextMeshProUGUI label;

    [Header("Smooth")]
    [SerializeField] private float smoothSpeed = 10f;

    private PlayerHealth boundHealth;
    private float targetFill = 1f;

    private void Start()
    {
        // если Bind() не вызывали — ищем PlayerHealth на том же объекте
        if (boundHealth == null)
        {
            boundHealth = GetComponent<PlayerHealth>();
            if (boundHealth != null)
            {
                boundHealth.OnHealthChanged += UpdateView;
                UpdateView(boundHealth.CurrentHp, boundHealth.MaxHp);
            }
            else
            {
                Debug.LogWarning("HPBarUI: PlayerHealth не привязан и не найден на объекте.", this);
            }
        }
    }

    private void OnDestroy()
    {
        if (boundHealth != null)
            boundHealth.OnHealthChanged -= UpdateView;
    }

    // ===== Публичный API =====

    public void Bind(PlayerHealth health)
    {
        if (boundHealth != null)
            boundHealth.OnHealthChanged -= UpdateView;

        boundHealth = health;

        if (boundHealth != null)
        {
            boundHealth.OnHealthChanged += UpdateView;
            UpdateView(boundHealth.CurrentHp, boundHealth.MaxHp);
        }
    }

    public void UpdateImmediate(float current, float max)
    {
        UpdateView(current, max);
        if (fillImage != null)
            fillImage.fillAmount = targetFill;
    }

    // ==========================

    private void UpdateView(float current, float max)
    {
        targetFill = max > 0f ? current / max : 0f;

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
