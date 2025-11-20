using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnergyBarUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;      // energy_fill (Image Type = Filled)
    [SerializeField] private Image frameImage;     // energy_frame (Sliced, опционально)
    [SerializeField] private Image iconImage;      // lightning icon (опционально)
    [SerializeField] private TextMeshProUGUI label;

    [Header("Smooth")]
    [SerializeField] private float smoothSpeed = 10f;

    private PlayerEnergy boundEnergy;
    private float targetFill = 1f;

    private void Start()
    {
        // если Bind() не вызывали — попробуем найти PlayerEnergy на том же объекте
        if (boundEnergy == null)
        {
            boundEnergy = GetComponent<PlayerEnergy>();
            if (boundEnergy != null)
            {
                boundEnergy.OnEnergyChanged += UpdateView;
                UpdateView(boundEnergy.CurrentEnergy, boundEnergy.MaxEnergy);
            }
            else
            {
                Debug.LogWarning("EnergyBarUI: PlayerEnergy не привязан и не найден на объекте.", this);
            }
        }
    }

    private void OnDestroy()
    {
        if (boundEnergy != null)
            boundEnergy.OnEnergyChanged -= UpdateView;
    }

    // ===== Публичный API (совместим со старым кодом) =====

    public void Bind(PlayerEnergy energy)
    {
        // снимаем старую подписку, если была
        if (boundEnergy != null)
            boundEnergy.OnEnergyChanged -= UpdateView;

        boundEnergy = energy;

        if (boundEnergy != null)
        {
            boundEnergy.OnEnergyChanged += UpdateView;
            UpdateView(boundEnergy.CurrentEnergy, boundEnergy.MaxEnergy);
        }
    }

    public void UpdateImmediate(float current, float max)
    {
        UpdateView(current, max);
        if (fillImage != null)
            fillImage.fillAmount = targetFill; // сразу выставляем без Lerp
    }

    // =====================================================

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
