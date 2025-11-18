using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffIconUI : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public Image radialFill;      // круговой таймер
    public TextMeshProUGUI timerLabel;

    private BuffInstance buff;

    /// <summary>
    /// Привязать UI к баффу
    /// </summary>
    public void Bind(BuffInstance buff)
    {
        this.buff = buff;
        icon.sprite = buff.Icon;

        UpdateUI();
    }

    private void Update()
    {
        if (buff == null) return;

        UpdateUI();
    }

    private void UpdateUI()
    {
        float remain = buff.Remaining;

        // Круговой таймер (0–1)
        radialFill.fillAmount = buff.Progress01;

        // Текст таймера
        timerLabel.text = remain < 1f
            ? $"{remain:0.0}"
            : $"{remain:0}";
    }
}
