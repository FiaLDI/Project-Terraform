using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnergyBarUI : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI label;

    private PlayerEnergy boundEnergy;

    public void Bind(PlayerEnergy energy)
    {
        boundEnergy = energy;
        energy.OnEnergyChanged += UpdateView;
    }

    public void UpdateImmediate(float current, float max)
    {
        UpdateView(current, max);
    }

    private void UpdateView(float current, float max)
    {
        if (slider != null)
        {
            slider.maxValue = max;
            slider.value = current;
        }

        if (label != null)
        {
            label.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
        }
    }
}
