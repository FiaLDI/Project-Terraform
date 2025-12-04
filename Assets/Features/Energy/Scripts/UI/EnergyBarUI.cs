using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Features.Stats.Domain;

public class EnergyBarUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI label;

    [Header("Smooth")]
    [SerializeField] private float smoothSpeed = 10f;

    private IEnergyView energy;
    private float targetFill = 1f;

    private void OnDestroy()
    {
        if (energy != null)
            energy.OnEnergyChanged -= UpdateView;
    }

    public void Bind(IEnergyView e)
    {
        if (energy != null)
            energy.OnEnergyChanged -= UpdateView;

        energy = e;

        if (energy != null)
        {
            energy.OnEnergyChanged += UpdateView;
            UpdateView(energy.CurrentEnergy, energy.MaxEnergy);
        }
        else
        {
            Debug.LogWarning("[EnergyBarUI] Bind() received null reference!");
        }
    }

    private void UpdateView(float current, float max)
    {
        targetFill = (max > 0) ? current / max : 0f;

        if (label != null)
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
